namespace Lyt.Jigsaw.Workflow.Shared;

public sealed partial class ThumbnailViewModel : ViewModel<ThumbnailView>, IRecipient<LanguageChangedMessage>
{
    public const double LargeFontSize = 24.0;
    public const double SmallFontSize = 16.0;

    public const double LargeBorderHeight = 260;
    public const double SmallBorderHeight = 212;

    public const double LargeImageHeight = 200;
    public const double SmallImageHeight = 160;

    public const int LargeThumbnailWidth = 360;
    public const int SmallThumbnailWidth = 240;

    public readonly PictureMetadata Metadata;
    public readonly byte[] ImageBytes;

    private readonly ISelectListener parent;
    private readonly bool isLarge;

    [ObservableProperty]
    private double fontSize;

    [ObservableProperty]
    private double borderHeight;

    [ObservableProperty]
    private double imageHeight;

    [ObservableProperty]
    private string provider;

    [ObservableProperty]
    private WriteableBitmap thumbnail;

    /// <summary> 
    /// Creates a thumbnail from a full (large) image - use for downloads 
    /// - OR - 
    /// Creates a thumbnail from a small (thumbnail) image - use for collection 
    /// </summary>
    public ThumbnailViewModel(
        ISelectListener parent, 
        PictureMetadata metadata, byte[] imageBytes, bool isLarge = true )        
    {
        this.parent = parent;
        this.Metadata = metadata;
        this.ImageBytes = imageBytes;
        this.isLarge = isLarge; 
        this.BorderHeight = isLarge ? LargeBorderHeight : SmallBorderHeight;
        this.ImageHeight = isLarge ? LargeImageHeight : SmallImageHeight;
        this.FontSize = isLarge ? LargeFontSize : SmallFontSize;
        this.Provider = string.Empty;
        this.SetThumbnailTitle(); 
        var bitmap =
            isLarge  ?
                WriteableBitmap.DecodeToWidth(
                    new MemoryStream(imageBytes), 
                    isLarge ? LargeThumbnailWidth : SmallThumbnailWidth) :
                WriteableBitmap.Decode(new MemoryStream(imageBytes));
        this.Thumbnail = bitmap;
        this.Subscribe<LanguageChangedMessage>();
    }

    // We need to reload the thumbnail view title, so that it will be properly localized
    public void Receive(LanguageChangedMessage _) => this.SetThumbnailTitle(); 

    internal void OnSelect() => this.parent.OnSelect(this);

    internal void ShowDeselected(PictureMetadata metadata)
    {
        if (this.Metadata == metadata)
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
        var model = App.GetRequiredService<JigsawModel>();
        string? currentLanguage = this.Localizer.CurrentLanguage;
        if (!string.IsNullOrEmpty(currentLanguage))
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentLanguage);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentLanguage);
        }

        string dateString = this.Metadata.Date.ToShortDateString();

        // TODO 
        //
        //string providerName = model.ProviderName(this.Metadata.Provider);
        //string providerLocalized = this.Localize(providerName, failSilently: true);
        //this.Provider =
        //    this.isLarge ?
        //        providerLocalized :
        //        string.Concat(providerLocalized, "  ~  ", dateString );
    }
}
