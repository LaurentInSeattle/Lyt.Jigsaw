namespace Lyt.Jigsaw;

public partial class App : ApplicationBase
{
    public const string Organization = "Lyt";
    public const string Application = "Jigsaw";
    public const string RootNamespace = "Lyt.Jigsaw";
    public const string AssemblyName = "Lyt.Jigsaw";
    public const string AssetsFolder = "Assets";

    public App() : base(
        App.Organization,
        App.Application,
        App.RootNamespace,
        typeof(MainWindow),
        typeof(ApplicationModelBase), // Top level model 
        [
            // Models 
            typeof(FileManagerModel),
            typeof(JigsawModel),
        ],
        [
           // Singletons
           typeof(ShellViewModel),
           typeof(PuzzleViewModel),
           typeof(PuzzleToolbarViewModel),
           typeof(CollectionViewModel),
           typeof(CollectionToolbarViewModel),
           typeof(IntroViewModel),
           typeof(IntroToolbarViewModel),
           typeof(LanguageViewModel),
           typeof(LanguageToolbarViewModel),
        ],
        [
            // Services 
            App.LoggerService,
            Service<IAnimationService, AnimationService>(),
            Service<ILocalizer, LocalizerModel>(),
            Service<IDialogService, DialogService >(),
            Service<IDispatch, Dispatch>(),
            Service<IProfiler, Profiler>(),
            Service<IToaster, Toaster>(),
            Service<IRandomizer, Randomizer>(),
        ],
        singleInstanceRequested: false,
        splashImageUri: null,
        appSplashWindow: new SplashWindow()
        )
    {
        // This should be empty, use the OnStartup override
        Instance = this;
        Debug.WriteLine("App Instance created");
    }

#pragma warning disable CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor. 
    public static App Instance { get; private set; }
#pragma warning restore CS8618 

    private static Tuple<Type, Type> LoggerService =>
            Debugger.IsAttached ?
                Service<ILogger, LogViewerWindow>() :
                Service<ILogger, Logger>();

    public bool RestartRequired { get; set; }

    protected override async Task OnStartupBegin()
    {
        ViewModel.TypeInitialize(ApplicationBase.AppHost);

        var logger = App.GetRequiredService<ILogger>();
        logger.Debug("OnStartupBegin begins");

        // This needs to complete before all models are initialized.
        var fileManager = App.GetRequiredService<FileManagerModel>();
        await fileManager.Configure(
            new FileManagerConfiguration(
                App.Organization, App.Application, App.RootNamespace, App.AssemblyName, App.AssetsFolder));

        // The localizer needs the File Manager, do not change the order.
        var localizer = App.GetRequiredService<ILocalizer>();
        await localizer.Configure(
            new LocalizerConfiguration
            {
                AssemblyName = App.AssemblyName,
                Languages =
                [
                    // Master, See JigsawLanguages.json in Tools folder 
                    "en-US", 

                    // Auto Translated 
                    "fr-FR", "it-IT", "es-ES", "de-DE",
                    "uk-UA", "bg-BG", "el-GR", "hy-AM",
                    "jp-JP", "ko-KO", "zh-CN", "zh-TW",
                    "hi-IN", "bn-BD", "hu-HU",
                ],
                // Use default for all other config parameters of the Localizer 
            });

        logger.Debug("OnStartupBegin complete");
    }

    protected override Task OnShutdownComplete()
    {
        var logger = App.GetRequiredService<ILogger>();
        logger.Debug("On Shutdown Complete");

        if (this.RestartRequired)
        {
            logger.Debug("On Shutdown Complete: Restart Required");
            var process = Process.GetCurrentProcess();
            if ((process is not null) && (process.MainModule is not null))
            {
                Process.Start(process.MainModule.FileName);
            }
        }

        return Task.CompletedTask;
    }

    // Why does it need to be there ??? 
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
