namespace Lyt.Jigsaw.Model;

using Lyt.Jigsaw.Model.Utilities;
using static Lyt.Persistence.FileManagerModel;

public sealed partial class JigsawModel : ModelBase
{
    public const string DefaultLanguage = "fr-FR";
    private const string JigsawModelFilename = "JigsawData";

    private static readonly JigsawModel DefaultData =
        new()
        {
            Language = DefaultLanguage,
            IsFirstRun = true,
            MaxImages = 128,
            MaxStorageMB = 64,
            MaxImageWidth = 3840,
            ShouldAutoCleanup = true,
            //Providers =
            //[
            //    new Provider(ImageProviderKey.Personal, "Model.Provider.Personal") { IsSelected = false},
            //    new Provider(ImageProviderKey.Epic, "Nasa EPIC"),
            //    new Provider(ImageProviderKey.Nasa, "Nasa APOD"),
            //    new Provider(ImageProviderKey.Bing, "Bing Wallpaper"),
            //    new Provider(ImageProviderKey.EarthView, "Google Earth View"),
            //    new Provider(ImageProviderKey.OpenVerse, "OpenVerse.Org"),
            //]
        };

    private readonly FileManagerModel fileManager;
    private readonly TranslatorService translatorService;
    private readonly ILocalizer localizer;
    private readonly Lock lockObject = new();
    private readonly FileId modelFileId;
    private readonly TimeoutTimer timeoutTimer; 

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public JigsawModel() : base(null)
    {
        this.modelFileId = new FileId(Area.User, Kind.Json, JigsawModel.JigsawModelFilename);
        // Do not inject the FileManagerModel instance: a parameter-less ctor is required for Deserialization 
        // Empty CTOR required for deserialization 
        this.ShouldAutoSave = false;
    }
#pragma warning restore CS8625 
#pragma warning restore CS8618

    public JigsawModel(
        FileManagerModel fileManager,
        TranslatorService translatorService,
        ILocalizer localizer,
        ILogger logger) : base(logger)
    {
        this.fileManager = fileManager;
        this.translatorService = translatorService;
        this.localizer = localizer;
        this.modelFileId = new FileId(Area.User, Kind.Json, JigsawModel.JigsawModelFilename);
        this.timeoutTimer = new TimeoutTimer(this.OnSavePuzzle, timeoutMilliseconds: 20_000);
        this.ShouldAutoSave = true;
    }

    ~JigsawModel()
    {
        NetworkChange.NetworkAvailabilityChanged -= this.OnNetworkAvailabilityChanged;
    }

    public override async Task Initialize()
    {
        this.IsInitializing = true;
        await this.Load();
        this.IsInitializing = false;
        this.IsDirty = false;
    }

    public override async Task Shutdown()
    {
        // Force a save on shutdown 
        this.SavePuzzle();
        this.SaveGame();

        if (this.IsDirty)
        {
            await this.Save();
        }
    }

    public Task Load()
    {
        try
        {
            if (!this.fileManager.Exists(this.modelFileId))
            {
                this.fileManager.Save(this.modelFileId, JigsawModel.DefaultData);
            }

            JigsawModel model = this.fileManager.Load<JigsawModel>(this.modelFileId);

            // Copy all properties with attribute [JsonRequired]
            base.CopyJSonRequiredProperties<JigsawModel>(model);

            //this.ValidateCollection();
            //this.CleanupCollection();
            // Load the thumbnails
            // Task.Run(this.LoadThumbnailCache);

            // Check Internet by send a fire and forget ping request to Azure 
            this.IsInternetConnected = false;
            _ = this.Ping();
            NetworkChange.NetworkAvailabilityChanged += this.OnNetworkAvailabilityChanged;

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            string msg = "Failed to load Model from " + this.modelFileId.Filename;
            this.Logger.Fatal(msg);
            throw new Exception("", ex);
        }
    }

    public override Task Save()
    {
        // Null check is needed !
        // If the File Manager is null we are currently loading the model and activating properties on a second instance 
        // causing dirtyness, and in such case we must avoid the null crash and anyway there is no need to save anything.
        if (this.fileManager is not null)
        {
#if DEBUG 
            if (this.fileManager.Exists(this.modelFileId))
            {
                this.fileManager.Duplicate(this.modelFileId);
            }
#endif // DEBUG 

            this.fileManager.Save(this.modelFileId, this);

#if DEBUG 
            try
            {
                string path = this.fileManager.MakePath(this.modelFileId);
                var fileInfo = new FileInfo(path);
                if (fileInfo.Length < 1024)
                {
                    // if (Debugger.IsAttached) { Debugger.Break(); }
                    this.Logger.Warning("Model file is too small!");
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) { Debugger.Break(); }
                Debug.WriteLine(ex);
            }
#endif // DEBUG 

            base.Save();
        }

        return Task.CompletedTask;
    }

    private const int PingTimeout = 12_000;
    private const string PingHost = "www.bing.com";

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
        => _ = this.Ping(); // Fire and forget 

    private async Task Ping()
    {
        void Trouble(Exception ex)
        {
            string message = ex.Message + "\n" + ex.ToString();
            Debug.WriteLine(message);
            this.Logger.Warning(message);
            this.IsInternetConnected = false;
        }

        try
        {
            using Ping ping = new();
            PingReply reply = await ping.SendPingAsync(PingHost, PingTimeout);
            this.IsInternetConnected = (reply is { Status: IPStatus.Success });
            string message = this.IsInternetConnected ? "Service is available." : "No internet or server down";
            Debug.WriteLine(message);
            if (this.IsInternetConnected)
            {
                this.Logger.Info(message);
            }
            else
            {
                this.Logger.Warning(message);
            }
        }
        catch (PingException pex)
        {
            if (pex.InnerException is SocketException sex)
            {
                if (sex.SocketErrorCode == SocketError.NoData)
                {
                    // Stupid Azure does not Ping properly, assumes connected in this case
                    this.IsInternetConnected = true;
                    string message = "Service is available.";
                    Debug.WriteLine(message);
                    this.Logger.Info(message);
                    return;
                }
            }

            Trouble(pex);
        }
        catch (Exception ex)
        {
            Trouble(ex);
        }
        finally
        {
            this.PingComplete = true;
            this.NotifyModelLoaded();
        }
    }

    public void SelectLanguage(string languageKey)
    {
        this.Language = languageKey;
        this.localizer.SelectLanguage(languageKey);
    }

    private void NotifyModelLoaded()
    {
        lock (this.lockObject)
        {
            if (!this.ModelLoadedNotified && this.ThumbnailsLoaded && this.PingComplete)
            {
                new ModelLoadedMessage().Publish();
                this.ModelLoadedNotified = true;
            }
        }
    }

}
