using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Skua.App.WPF.Services;
using Skua.Core.AppStartup;
using Skua.Core.Interfaces;
using Skua.WPF;
using Skua.WPF.Services;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Westwind.Scripting;

namespace Skua.App.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public new static App Current => (App)Application.Current;

    public IServiceProvider Services { get; }
    private readonly IScriptInterface _bot;

    public App()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        Services = ConfigureServices();
        Services.GetRequiredService<ISettingsService>().SetApplicationVersion("1.3.0.2");

        InitializeComponent();

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string targetPath = Path.Combine(appData, "Skua");

        Services.GetRequiredService<IClientFilesService>().CreateDirectories();
        Services.GetRequiredService<IClientFilesService>().CreateFiles();
        Task.Factory.StartNew(async () => await Services.GetRequiredService<IScriptServers>().GetServers());

        _bot = Services.GetRequiredService<IScriptInterface>();
        _ = Services.GetRequiredService<ILogService>();

        string[] args = Environment.GetCommandLineArgs();
        SkuaStartupHandler startup = new(args, _bot, Services.GetRequiredService<ISettingsService>(), Services.GetRequiredService<IThemeService>());
        startup.Execute();

        RoslynLifetimeManager.WarmupRoslyn();
        Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = Services.GetRequiredService<ISettingsService>().Get<int>("AnimationFrameRate") });

        Application.Current.Exit += App_Exit;
    }

    private async void App_Exit(object? sender, EventArgs e)
    {
        Services.GetRequiredService<ICaptureProxy>().Stop();

        await ((IAsyncDisposable)Services.GetRequiredService<IScriptBoost>()).DisposeAsync();
        await ((IAsyncDisposable)Services.GetRequiredService<IScriptBotStats>()).DisposeAsync();
        await ((IAsyncDisposable)Services.GetRequiredService<IScriptDrop>()).DisposeAsync();
        await Ioc.Default.GetRequiredService<IScriptManager>().StopScriptAsync();
        await ((IScriptInterfaceManager)_bot).StopTimerAsync();

        Services.GetRequiredService<IFlashUtil>().Dispose();

        WeakReferenceMessenger.Default.Cleanup();
        WeakReferenceMessenger.Default.Reset();
        StrongReferenceMessenger.Default.Reset();

        RoslynLifetimeManager.ShutdownRoslyn();
        Application.Current.Exit -= App_Exit;
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "VSCode")))
        {
            Services.GetRequiredService<ISettingsService>().Set("UseLocalVSC", false);
        }

        MainWindow main = new() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
        Application.Current.MainWindow = main;
        main.Show();

        IDialogService dialogService = Services.GetRequiredService<IDialogService>();
        IGetScriptsService getScripts = Services.GetRequiredService<IGetScriptsService>();
        ILogService logService = Services.GetRequiredService<ILogService>();
        
        // Silent script update - no popup dialogs
        if (Services.GetRequiredService<ISettingsService>().Get<bool>("CheckBotScriptsUpdates"))
        {
            Task.Factory.StartNew(async () =>
            {
                await getScripts.GetScriptsAsync(null, default);

                if (getScripts.Missing > 0 || getScripts.Outdated > 0)
                {
                    int count = await getScripts.DownloadAllWhereAsync(s => !s.Downloaded || s.Outdated);
                    logService.ScriptLog($"[Auto-Update] Downloaded {count} scripts.");
                }
            });
        }

        // Silent AdvanceSkill Sets update - no popup dialogs
        if (Services.GetRequiredService<ISettingsService>().Get<bool>("CheckAdvanceSkillSetsUpdates"))
        {
            IAdvancedSkillContainer advanceSkillSets = Services.GetRequiredService<IAdvancedSkillContainer>();
            Task.Factory.StartNew(async () =>
            {
                long remoteSize = await getScripts.CheckAdvanceSkillSetsUpdates();
                if (remoteSize > 0)
                {
                    if (await getScripts.UpdateSkillSetsFile())
                    {
                        advanceSkillSets.SyncSkills();
                        logService.ScriptLog("[Auto-Update] AdvanceSkill Sets updated.");
                    }
                }
            });
        }

        Services.GetRequiredService<IPluginManager>().Initialize();

        Services.GetRequiredService<IHotKeyService>().Reload();
    }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private IServiceProvider ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddWindowsServices();

        services.AddCommonServices();

        services.AddScriptableObjects();

        services.AddCompiler();

        services.AddSkuaMainAppViewModels();

        ServiceProvider provider = services.BuildServiceProvider();
        Ioc.Default.ConfigureServices(provider);

        return provider;
    }
}