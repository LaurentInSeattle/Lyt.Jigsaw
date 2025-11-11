namespace Lyt.Jigsaw.Workflow.Shared;

public sealed partial class ThumbnailViewModel : ViewModel<ThumbnailView>, IRecipient<LanguageChangedMessage>
{
    public const double LargeBorderHeight = 260;
    public const double LargeImageHeight = 200;
    public const int LargeThumbnailWidth = 360;

    public readonly Model.GameObjects.Game Game;

    public readonly byte[] ImageBytes;

    private readonly ISelectListener parent;

    [ObservableProperty]
    private double borderHeight;

    [ObservableProperty]
    private double imageHeight;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string details;

    [ObservableProperty]
    private WriteableBitmap thumbnail;

    /// <summary>  Creates a thumbnail view model </summary>
    public ThumbnailViewModel(ISelectListener parent, Model.GameObjects.Game game, byte[] imageBytes)        
    {
        this.parent = parent;
        this.Game = game;
        this.ImageBytes = imageBytes;
        this.BorderHeight = LargeBorderHeight ;
        this.ImageHeight = LargeImageHeight;
        this.Title = string.Empty;
        this.Details = string.Empty;
        this.SetThumbnailTitle(); 
        this.Thumbnail = WriteableBitmap.Decode(new MemoryStream(imageBytes));
        this.Subscribe<LanguageChangedMessage>();
    }

    // We need to reload the thumbnail view title, so that it will be properly localized
    public void Receive(LanguageChangedMessage _) => this.SetThumbnailTitle(); 

    internal void OnSelect() => this.parent.OnSelect(this);

    internal void ShowDeselected(Model.GameObjects.Game game)
    {
        if (this.Game == game)
        {
            return;
        }

        if (this.IsBound)
        {
            this.View.Deselect();
        }
    }

    internal void ShowSelected()
    {
        if (this.IsBound)
        {
            this.View.Select();
        } 
    }

    private void SetThumbnailTitle()
    {
        string? currentLanguage = this.Localizer.CurrentLanguage;
        if (!string.IsNullOrEmpty(currentLanguage))
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentLanguage);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentLanguage);
        }

        string dateString = this.Game.Started.Date.ToShortDateString();
        this.Title = dateString;
    }
}
