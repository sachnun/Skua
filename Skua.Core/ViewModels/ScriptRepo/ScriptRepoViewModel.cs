using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Skua.Core.Interfaces;
using Skua.Core.Messaging;
using Skua.Core.Models.GitHub;
using Skua.Core.Utils;

namespace Skua.Core.ViewModels;

public partial class ScriptRepoViewModel : BotControlViewModelBase
{
    public ScriptRepoViewModel(IGetScriptsService getScripts, IProcessService processService)
        : base("Search Scripts", 800, 450)
    {
        _getScriptsService = getScripts;
        _processService = processService;
        OpenScriptFolderCommand = new RelayCommand(_processService.OpenVSC);
    }

    private bool _isInitialized;
    private CancellationTokenSource? _syncCts;

    private readonly IGetScriptsService _getScriptsService;
    private readonly IProcessService _processService;

    [ObservableProperty]
    private bool _isManagerMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadedQuantity), nameof(OutdatedQuantity), nameof(ScriptQuantity), nameof(BotScriptQuantity))]
    private RangedObservableCollection<ScriptInfoViewModel> _scripts = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadedQuantity), nameof(OutdatedQuantity), nameof(ScriptQuantity), nameof(BotScriptQuantity))]
    private ScriptInfoViewModel? _selectedItem;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _progressReportMessage = string.Empty;

    public int DownloadedQuantity => _getScriptsService?.Downloaded ?? 0;
    public int OutdatedQuantity => _getScriptsService?.Outdated ?? 0;
    public int ScriptQuantity => _getScriptsService?.Total ?? 0;
    public int BotScriptQuantity => _scripts.Count;
    public IRelayCommand OpenScriptFolderCommand { get; }

    /// <summary>
    /// Initialize and load scripts. Call this when the view becomes visible.
    /// </summary>
    public void Initialize()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            _ = AutoSyncAsync();
        }
    }

    protected override void OnActivated()
    {
        Initialize();
    }

    /// <summary>
    /// Automatically fetches scripts, updates outdated ones, and refreshes the list.
    /// </summary>
    private async Task AutoSyncAsync()
    {
        _syncCts?.Cancel();
        _syncCts = new CancellationTokenSource();
        CancellationToken token = _syncCts.Token;

        IsBusy = true;

        try
        {
            // Step 1: Fetch script list from remote
            ProgressReportMessage = "Fetching scripts...";
            await Task.Run(async () =>
            {
                Progress<string> progress = new(ProgressHandler);
                await _getScriptsService.GetScriptsAsync(progress, token);
            }, token);

            if (token.IsCancellationRequested)
                return;

            // Step 2: Refresh UI list
            RefreshScriptsList();

            // Step 3: Auto-update outdated scripts silently
            int outdated = _getScriptsService?.Outdated ?? 0;
            if (outdated > 0)
            {
                ProgressReportMessage = $"Updating {outdated} scripts...";
                int count = await _getScriptsService!.DownloadAllWhereAsync(s => s.Outdated);
                RefreshScriptsList();
            }

            ProgressReportMessage = string.Empty;
        }
        catch (OperationCanceledException)
        {
            // Cancelled, ignore
        }
        catch
        {
            ProgressReportMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefreshScriptsList()
    {
        List<ScriptInfoViewModel> newScripts = new();
        if (_getScriptsService?.Scripts != null)
        {
            foreach (ScriptInfo script in _getScriptsService.Scripts)
            {
                if (script?.Name != null && !script.Name.Equals("null"))
                {
                    if (script.Description?.Equals("null") == true)
                        script.Description = "No description provided.";

                    if (script.Tags?.Contains("null") == true && (script.Tags.Length == 1))
                        script.Tags = new[] { "no-tags" };
                    else if (script.Tags == null)
                        script.Tags = new[] { "no-tags" };

                    newScripts.Add(new(script));
                }
            }
        }

        _scripts.ReplaceRange(newScripts);

        OnPropertyChanged(nameof(Scripts));
        OnPropertyChanged(nameof(DownloadedQuantity));
        OnPropertyChanged(nameof(OutdatedQuantity));
        OnPropertyChanged(nameof(ScriptQuantity));
        OnPropertyChanged(nameof(BotScriptQuantity));
    }

    public void ProgressHandler(string message)
    {
        ProgressReportMessage = message;
    }

    [RelayCommand]
    private void OpenScript()
    {
        if (SelectedItem is null || !SelectedItem.Downloaded)
            return;

        StrongReferenceMessenger.Default.Send<EditScriptMessage, int>(new(SelectedItem.LocalFile), (int)MessageChannels.ScriptStatus);
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_selectedItem is null)
            return;

        IsBusy = true;
        ProgressReportMessage = $"Deleting {_selectedItem.FileName}...";
        await _getScriptsService.DeleteScriptAsync(_selectedItem.Info);
        _selectedItem.Downloaded = false;
        OnPropertyChanged(nameof(DownloadedQuantity));
        OnPropertyChanged(nameof(OutdatedQuantity));
        ProgressReportMessage = string.Empty;
        IsBusy = false;
    }

    [RelayCommand]
    private async Task Download()
    {
        if (_selectedItem is null)
            return;

        IsBusy = true;
        ProgressReportMessage = $"Downloading {_selectedItem.FileName}...";
        await _getScriptsService.DownloadScriptAsync(_selectedItem.Info);
        _selectedItem.Downloaded = true;
        OnPropertyChanged(nameof(DownloadedQuantity));
        OnPropertyChanged(nameof(OutdatedQuantity));
        ProgressReportMessage = string.Empty;
        IsBusy = false;
    }

    [RelayCommand]
    public void CancelTask()
    {
        _syncCts?.Cancel();
        ProgressReportMessage = string.Empty;
        IsBusy = false;
    }
}
