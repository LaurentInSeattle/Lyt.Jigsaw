namespace Lyt.Jigsaw.Shell;

using static Messaging.ApplicationMessagingExtensions;

public sealed partial class ShellViewModel 
    : ViewModel<ShellView>,
    IRecipient<ToolbarCommandMessage>,
    IRecipient<LanguageChangedMessage>
{
    private const int MinutesToMillisecs = 60 * 1_000;

    private readonly JigsawModel jigsawModel;
    private readonly IToaster toaster;

    [ObservableProperty]
    public bool mainToolbarIsVisible;

    private ViewSelector<ActivatedView>? viewSelector;
    public bool isFirstActivation;

    #region To please the XAML viewer 

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // Should never be executed 
    public ShellViewModel()
    {
    }
#pragma warning restore CS8618 

    #endregion To please the XAML viewer 

    public ShellViewModel(JigsawModel astroPicModel, IToaster toaster)
    {
        this.jigsawModel = astroPicModel;
        this.toaster = toaster;

        //this.Messenger.Subscribe<ViewActivationMessage>(this.OnViewActivation);
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<LanguageChangedMessage>();
    }

    public void Receive(LanguageChangedMessage _) 
    {
    }

    public void Receive(ToolbarCommandMessage _) { } 

    public override void OnViewLoaded()
    {
        this.Logger.Debug("OnViewLoaded begins");

        base.OnViewLoaded();
        if (this.View is null)
        {
            throw new Exception("Failed to startup...");
        }

        // Select default language 
        string preferredLanguage = this.jigsawModel.Language;
        this.Logger.Debug("Language: " + preferredLanguage);
        this.Localizer.SelectLanguage(preferredLanguage);
        Thread.CurrentThread.CurrentCulture = new CultureInfo(preferredLanguage);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(preferredLanguage);

        this.Logger.Debug("OnViewLoaded language loaded");

        // Create all statics views and bind them 
        this.SetupWorkflow();
        this.Logger.Debug("OnViewLoaded SetupWorkflow complete");

        // Ready 
        this.toaster.Host = this.View.ToasterHost;
        if (true)
        {
            this.toaster.Show(
                this.Localize("Shell.Ready"), this.Localize("Shell.Greetings"),
                1_600, InformationLevel.Info);
        }

        // Delay a bit the launch of the gallery so that there is time to ping 
        //this.Logger.Debug("OnViewLoaded: Internet connected: " + this.jigsawModel.IsInternetConnected);
        //Schedule.OnUiThread(100, this.ActivateInitialView, DispatcherPriority.Background);

        this.Logger.Debug("OnViewLoaded complete");

        var puzzle = new Puzzle(1080, 1920, 0);
        var counts = puzzle.PieceCounts;
        puzzle.Setup(counts[counts.Count / 2], rotationSteps :0 );
        Schedule.OnUiThread(100, this.TestPoints, DispatcherPriority.Background);
    }

    private void TestPoints()
    {
        List<Point> hpoints =
        [
            // Base
            //new (0, 0),
            //new (300, 20),
            //new (350, -120),
            //new (450, -120),
            //new (500, 20),
            //new (800, 0),

            // Variant
            new (0, 0),
            new (300, 20),
            new (350, -160),
            new (450, -100),
            new (500, 20),
            new (800, 0),

            // Variant
            //new (0, 0),
            //new (350, 20),
            //new (350, -120),
            //new (450, -120),
            //new (480, 30),
            //new (800, 0),
        ];

        List<Point> vpoints = [];
        foreach (var point  in hpoints)
        {
            vpoints.Add(new Point(point.Y, point.X)); 
        }

        this.View.PathSegments.Data = GeometryGenerator.Segments(vpoints, isClosed: false);
        this.View.PathPoints.Data = GeometryGenerator.BezierControlPoints(vpoints, isClosed: false);
        this.View.PathCurve.Data = GeometryGenerator.CatmullRom(vpoints, isClosed: false);
    }


    private async void ActivateInitialView()
    {
        this.isFirstActivation = true;

        if (this.jigsawModel.IsFirstRun)
        {
            Select(ActivatedView.Language);
        }
        else
        {
            int retries = 3;
            while (retries > 0)
            {
                this.Logger.Debug("ActivateInitialView: Internet connected: " + this.jigsawModel.IsInternetConnected);
                if (this.jigsawModel.IsInternetConnected)
                {
                    Select(ActivatedView.Gallery);
                    this.Logger.Debug("OnViewLoaded OnViewActivation complete");
                    return;
                }

                await Task.Delay(100);
                --retries;
            }
        }

        this.Logger.Debug("OnViewLoaded OnViewActivation complete");
    }

    private void SetupWorkflow()
    {
        if (this.View is not ShellView view)
        {
            throw new Exception("No view: Failed to startup...");
        }

        var selectableViews = new List<SelectableView<ActivatedView>>();

        void Setup<TViewModel, TControl, TToolbarViewModel, TToolbarControl>(
                ActivatedView activatedView, Control control)
            where TViewModel : ViewModel<TControl>
            where TControl : Control, IView, new()
            where TToolbarViewModel : ViewModel<TToolbarControl>
            where TToolbarControl : Control, IView, new()
        {
            var vm = App.GetRequiredService<TViewModel>();
            vm.CreateViewAndBind();
            var vmToolbar = App.GetRequiredService<TToolbarViewModel>();
            vmToolbar.CreateViewAndBind();
            selectableViews.Add(
                new SelectableView<ActivatedView>(activatedView, vm, control, vmToolbar));
        }

        //Setup<GalleryViewModel, GalleryView, GalleryToolbarViewModel, GalleryToolbarView>(
        //    ActivatedView.Gallery, view.TodayButton);

        //Setup<CollectionViewModel, CollectionView, CollectionToolbarViewModel, CollectionToolbarView>(
        //    ActivatedView.Collection, view.CollectionButton);

        //Setup<IntroViewModel, IntroView, IntroToolbarViewModel, IntroToolbarView>(
        //    ActivatedView.Intro, view.IntroButton);

        //Setup<LanguageViewModel, LanguageView, LanguageToolbarViewModel, LanguageToolbarView>(
        //    ActivatedView.Language, view.FlagButton);

        //Setup<SettingsViewModel, SettingsView, SettingsToolbarViewModel, SettingsToolbarView>(
        //    ActivatedView.Settings, view.SettingsButton);

        // Needs to be kept alive as a class member, or else callbacks will die (and wont work) 
        this.viewSelector =
            new ViewSelector<ActivatedView>(
                this.View.ShellViewContent,
                this.View.ShellViewToolbar,
                this.View.SelectionGroup,
                selectableViews,
                this.OnViewSelected);
    }

    private void OnViewSelected(ActivatedView activatedView)
    {
        if (this.viewSelector is null)
        {
            throw new Exception("No view selector");
        }

        var newViewModel = this.viewSelector.CurrentPrimaryViewModel;
        if (newViewModel is not null)
        {
            bool mainToolbarIsHidden = false;
                // this.astroPicModel.IsFirstRun || newViewModel is IntroViewModel;
            this.MainToolbarIsVisible = !mainToolbarIsHidden;
            if (this.isFirstActivation)
            {
                this.Profiler.MemorySnapshot(newViewModel.ViewBase!.GetType().Name + ":  Activated");
            }
        }

        this.isFirstActivation = false;
    }

#pragma warning disable IDE0079 
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnToday() => Select(ActivatedView.Gallery);

    [RelayCommand]
    public void OnCollection() => Select(ActivatedView.Collection);

    [RelayCommand]
    public void OnSettings() => Select(ActivatedView.Settings);

    [RelayCommand]
    public void OnInfo() => Select(ActivatedView.Intro);

    [RelayCommand]
    public void OnLanguage() => Select(ActivatedView.Language);

    [RelayCommand]
    public void OnClose() => OnExit();

    private static async void OnExit()
    {
        var application = App.GetRequiredService<IApplicationBase>();
        await application.Shutdown();
    }
#pragma warning restore CA1822
#pragma warning restore IDE0079
}
