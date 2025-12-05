namespace Lyt.Jigsaw.Model;

public sealed partial class JigsawModel : ModelBase
{
    #region Serialized -  No model changed event

    [JsonRequired]
    public string Language { get => this.Get<string>()!; set => this.Set(value); }

    /// <summary> This should stay true, ==> But... Just FOR NOW !  </summary>
    [JsonRequired]
    public bool IsFirstRun { get; set; } = false;

    [JsonRequired]
    public int MaxImages { get => this.Get<int>(); set => this.Set(value); }

    [JsonRequired]
    public int MaxStorageMB { get => this.Get<int>(); set => this.Set(value); }

    [JsonRequired]
    public int MaxImageWidth { get => this.Get<int>(); set => this.Set(value); }

    [JsonRequired]
    public bool ShouldAutoCleanup { get => this.Get<bool>(); set => this.Set(value); }

    #endregion Serialized -  No model changed event

    #region Not serialized - No model changed event

    [JsonIgnore]
    public Game? Game { get; set; }

    [JsonIgnore]
    public Puzzle? Puzzle { get; set; }

    [JsonIgnore]
    public Dictionary<string, Game> SavedGames { get; set; } = [];

    [JsonIgnore]
    public Dictionary<string, byte[]> ThumbnailCache { get; set; } = [];

    [JsonIgnore]
    public bool ThumbnailsLoaded { get; set; } = false;

    [JsonIgnore]
    public bool PingComplete { get; set; } = false;

    [JsonIgnore]
    public bool ModelLoadedNotified { get; set; } = false;

    [JsonIgnore]
    public bool ShowInProgress { get; set; } = true;

    #endregion Not serialized - No model changed event

    #region NOT serialized - WITH model changed event

    [JsonIgnore]
    // Asynchronous: Must raise Model Updated events 
    public bool IsInternetConnected { get => this.Get<bool>(); set => this.Set(value); }

    #endregion NOT serialized - WITH model changed event    
}
