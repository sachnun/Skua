using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Skua.Core.Interfaces;
using Skua.Core.Messaging;
using Skua.Core.Models;

namespace Skua.Core.ViewModels;

public partial class ScriptLoaderViewModel : BotControlViewModelBase
{
    private readonly string _scriptPath;

    public ScriptLoaderViewModel(
        IProcessService processService,
        IFileDialogService fileDialog,
        IScriptManager scriptManager,
        IWindowService windowService,
        IDialogService dialogService,
        IEnumerable<LogTabViewModel> logs)
        : base("Load Script", 350, 450)
    {
        StrongReferenceMessenger.Default.Register<ScriptLoaderViewModel, LoadScriptMessage, int>(this, (int)MessageChannels.ScriptStatus, LoadScript);
        StrongReferenceMessenger.Default.Register<ScriptLoaderViewModel, EditScriptMessage, int>(this, (int)MessageChannels.ScriptStatus, EditScript);
        StrongReferenceMessenger.Default.Register<ScriptLoaderViewModel, StartScriptMessage, int>(this, (int)MessageChannels.ScriptStatus, StartScript);
        StrongReferenceMessenger.Default.Register<ScriptLoaderViewModel, ToggleScriptMessage, int>(this, (int)MessageChannels.ScriptStatus, ToggleScript);
        StrongReferenceMessenger.Default.Register<ScriptLoaderViewModel, ScriptStartedMessage, int>(this, (int)MessageChannels.ScriptStatus, ScriptStarted);
        StrongReferenceMessenger.Default.Register<ScriptLoaderViewModel, ScriptStoppedMessage, int>(this, (int)MessageChannels.ScriptStatus, ScriptStopped);
        StrongReferenceMessenger.Default.Register<ScriptLoaderViewModel, ScriptStoppingMessage, int>(this, (int)MessageChannels.ScriptStatus, ScriptStopping);

        _scriptPath = ClientFileSources.SkuaScriptsDIR;
        ScriptLogs = logs.ToArray()[1];
        ScriptManager = scriptManager;
        _windowService = windowService;
        _processService = processService;
        _dialogService = dialogService;
        _fileDialog = fileDialog;
    }

    public IScriptManager ScriptManager { get; }

    private readonly IWindowService _windowService;
    private readonly IProcessService _processService;
    private readonly IDialogService _dialogService;
    private readonly IFileDialogService _fileDialog;
    public LogTabViewModel ScriptLogs { get; }

    [ObservableProperty]
    private string _scriptErrorToolTip = string.Empty;

    [ObservableProperty]
    private bool _toggleScriptEnabled = true;

    [ObservableProperty]
    private string _loadedScript = "No script loaded";

    [RelayCommand]
    private void OpenBrowserForm()
    {
        _processService.OpenLink(@"https://discord.gg/Xz5bF6q2CX");
    }

    [RelayCommand]
    private void OpenScriptRepo()
    {
        // Toggle - send message to toggle visibility
        StrongReferenceMessenger.Default.Send(new ToggleScriptRepoMessage());
    }

    [RelayCommand]
    private void OpenVSCode()
    {
        _processService.OpenVSC();
    }

    private async Task StartScriptAsync(string? path = null, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        ScriptManager.SetLoadedScript(path);
        if (!string.IsNullOrWhiteSpace(name))
            LoadedScript = name;

        if (ScriptManager.ScriptRunning)
            await ScriptManager.StopScriptAsync();
        await StartScript();
    }

    [RelayCommand]
    private async Task ToggleScript()
    {
        ToggleScriptEnabled = false;
        if (string.IsNullOrWhiteSpace(ScriptManager.LoadedScript))
        {
            _dialogService.ShowMessageBox("No script loaded.", "Scripts");
            ToggleScriptEnabled = true;
            return;
        }

        if (ScriptManager.ScriptRunning)
        {
            ScriptManager.StopScript();
            ToggleScriptEnabled = true;
            return;
        }

        await StartScript();
    }

    private async Task StartScript()
    {
        await Task.Run(async () =>
        {
            Exception? ex = await ScriptManager.StartScriptAsync();
            if (ex is not null)
            {
                _dialogService.ShowMessageBox($"Error while starting script:\r\n{ex.Message}", "Script Error");
                ScriptErrorToolTip = $"Error while starting script:\r\n{ex}";
                ToggleScriptEnabled = true;
            }
        });
    }

    [RelayCommand]
    private void LoadScript()
    {
        string? path = _fileDialog.OpenFile(_scriptPath, "Skua Scripts (*.cs)|*.cs");
        if (path is null)
            return;
        string name = Path.GetFileNameWithoutExtension(path) ?? "Unknown";

        ScriptManager.SetLoadedScript(path);
        LoadedScript = name;
    }

    private void LoadScript(string path, string? name)
    {
        ScriptManager.SetLoadedScript(path);
        LoadedScript = name ?? Path.GetFileNameWithoutExtension(path) ?? "Unknown";
    }

    [RelayCommand]
    private void EditScript(string? path = null)
    {
        if (path is null && string.IsNullOrEmpty(ScriptManager.LoadedScript))
            return;

        _processService.OpenVSC(path ?? ScriptManager.LoadedScript);
    }

    [RelayCommand]
    private async Task EditScriptConfig()
    {
        if (string.IsNullOrWhiteSpace(ScriptManager.LoadedScript))
        {
            _dialogService.ShowMessageBox("No script is currently loaded. Please load a script to edit its options.", "No Script Loaded");
            return;
        }

        if (ScriptManager.ScriptRunning)
        {
            _dialogService.ShowMessageBox("Script currently running. Stop the script to change its options.", "Script Running");
            return;
        }

        try
        {
            object compiled = await Task.Run(() => ScriptManager.Compile(File.ReadAllText(ScriptManager.LoadedScript))!);
            ScriptManager.LoadScriptConfig(compiled);
            if (ScriptManager.Config!.Options.Count > 0 || ScriptManager.Config.MultipleOptions.Count > 0)
                ScriptManager.Config.Configure();
            else
                _dialogService.ShowMessageBox("The loaded script has no options to configure.", "No Options");
        }
        catch (Exception ex)
        {
            _dialogService.ShowMessageBox($"Script cannot be configured as it has compilation errors:\r\n{ex}", "Script Error");
        }
    }

    private async void ToggleScript(ScriptLoaderViewModel recipient, ToggleScriptMessage message)
    {
        await recipient.ToggleScript();
    }

    private async void StartScript(ScriptLoaderViewModel recipient, StartScriptMessage message)
    {
        string msgScriptName = message.Name ?? Path.GetFileNameWithoutExtension(message.Path) ?? "Unknown";
        string runningScriptMessage = $"Script {LoadedScript} is already running. Do you want to stop it?";
        bool startNew = false;

        ToggleScriptEnabled = false;

        if (ScriptManager.ScriptRunning)
        {
            if (Path.GetFileName(ScriptManager.LoadedScript) != Path.GetFileName(message.Path))
            {
                runningScriptMessage = $"{LoadedScript} is running. Do you want to stop it and start {msgScriptName}?";
                startNew = true;
            }

            DialogResult dialogResult = _dialogService.ShowMessageBox(runningScriptMessage, "Script Running", "No", "Yes");

            if (dialogResult.Text == "Yes")
            {
                ToggleScriptEnabled = false;
                await ScriptManager.StopScriptAsync();

                if (startNew)
                {
                    LoadedScript = msgScriptName;
                    await Task.Delay(5000);
                    await recipient.StartScriptAsync(message.Path, message.Name);
                }
            }

            ToggleScriptEnabled = true;
            return;
        }
        else
        {
            LoadedScript = msgScriptName;
        }

        await recipient.StartScriptAsync(message.Path, message.Name);
    }

    private void EditScript(ScriptLoaderViewModel recipient, EditScriptMessage message)
    {
        recipient.EditScript(message.Path);
    }

    private void LoadScript(ScriptLoaderViewModel recipient, LoadScriptMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.Path))
            recipient.LoadScript(message.Path, message.Name);
    }

    private void ScriptStopping(ScriptLoaderViewModel recipient, ScriptStoppingMessage message)
    {
        recipient.ToggleScriptEnabled = false;
    }

    private void ScriptStopped(ScriptLoaderViewModel recipient, ScriptStoppedMessage message)
    {
        recipient.ToggleScriptEnabled = true;
    }

    private void ScriptStarted(ScriptLoaderViewModel recipient, ScriptStartedMessage message)
    {
        recipient.ToggleScriptEnabled = true;
    }
}
