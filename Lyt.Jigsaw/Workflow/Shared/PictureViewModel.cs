namespace Lyt.Jigsaw.Workflow.Shared;

using static FileManagerModel;

public sealed partial class PictureViewModel : 
    ViewModel<PictureView>, 
    IRecipient<ZoomRequestMessage>
{
    public const int ThumbnailWidth = 280;

    private readonly JigsawModel jigsawModel;
    private readonly ViewModel parent;

    [ObservableProperty]
    private double zoomFactor;

    [ObservableProperty]
    private string provider;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string copyright;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private GridLength descriptionHeight;

    private PictureMetadata? pictureMetadata;
    private byte[]? imageBytes;
    private int imageWidth;

    public PictureViewModel(ViewModel parent)
    {
        this.parent = parent;
        this.jigsawModel = ApplicationBase.GetRequiredService<JigsawModel>();
        this.Subscribe<ZoomRequestMessage>();

        this.Provider = string.Empty;
        this.Title = string.Empty;
        this.Copyright = string.Empty;
        this.Description = string.Empty;
    }

    public override void OnViewLoaded()
    {
        base.OnViewLoaded();
        this.View.ZoomController.Tag = this.parent;
    }

    internal void Select(PictureMetadata pictureMetadata, byte[] imageBytes)
    {
        this.pictureMetadata = pictureMetadata;
        this.imageBytes = imageBytes;
        var bitmap = WriteableBitmap.Decode(new MemoryStream(imageBytes));
        this.imageWidth = (int)bitmap.Size.Width;
        this.LoadImage(bitmap);
        // TODO 
        //string providerName = this.jigsawModel.ProviderName(pictureMetadata.Provider);
        //this.Provider = this.Localize(providerName, failSilently: true);
        this.Title =
            string.IsNullOrWhiteSpace(pictureMetadata.Title) ? string.Empty : pictureMetadata.Title;
        this.Copyright =
            string.IsNullOrWhiteSpace(pictureMetadata.Copyright) ? string.Empty : pictureMetadata.Copyright;
        this.Description =
            string.IsNullOrWhiteSpace(pictureMetadata.Description) ? string.Empty : pictureMetadata.Description;
        double height = 0.0;
        if (!string.IsNullOrWhiteSpace(pictureMetadata.Description))
        {
            if (pictureMetadata.Description.Length < 150)
            {
                height = 40.0;
            }
            else if (pictureMetadata.Description.Length < 400)
            {
                height = 80.0;
            }
            else if (pictureMetadata.Description.Length < 800)
            {
                height = 120.0;
            }
            else
            {
                height = 180.0;
            }
        }

        this.DescriptionHeight = new GridLength(height, GridUnitType.Pixel);
        this.TranslateMetadata(pictureMetadata);
    }

    private void TranslateMetadata(PictureMetadata pictureMetadata)
    {
        string? currentLanguage = this.Localizer.CurrentLanguage;
        if (string.IsNullOrWhiteSpace(currentLanguage))
        {
            return;
        }

        if (currentLanguage == "en-US")
        {
            return;
        }

        // title translation is available: 
        if ((currentLanguage == pictureMetadata.TranslationLanguage) &&
         !string.IsNullOrWhiteSpace(pictureMetadata.TranslatedTitle))
        {
            this.Title = pictureMetadata.TranslatedTitle;
        }

        // description translation is available: 
        if ((currentLanguage == pictureMetadata.TranslationLanguage) &&
         !string.IsNullOrWhiteSpace(pictureMetadata.TranslatedDescription))
        {
            this.Description = pictureMetadata.TranslatedDescription;
        }
    }

    private void LoadImage(WriteableBitmap bitmap)
    {
        var image = new Image { Stretch = Stretch.Uniform };
        RenderOptions.SetBitmapInterpolationMode(image, BitmapInterpolationMode.MediumQuality);
        var canvas = this.View.Canvas;
        canvas.Children.Clear();
        canvas.Children.Add(image);
        canvas.Width = bitmap.Size.Width;
        canvas.Height = bitmap.Size.Height;
        image.Source = bitmap;

        // Enforce property changed but.. I dont understand why dispatch is needed 
        //this.View.ZoomController.Max;
        //Schedule.OnUiThread(
        //    50, () => { this.View.ZoomController.Min(); }, DispatcherPriority.Background);
    }

    internal void AddToCollection()
    {
        if (this.pictureMetadata is null || this.imageBytes is null)
        {
            return;
        }

        var writeableBitmap =
            WriteableBitmap.DecodeToWidth(new MemoryStream(this.imageBytes), ThumbnailWidth);
        byte[] thumbnailBytes = writeableBitmap.EncodeToJpeg();

        // Resize image if necessary
        int maxImageWidth = this.jigsawModel.MaxImageWidth;
        byte[] adjustedImageBytes = this.imageBytes;
        if (this.imageWidth > maxImageWidth)
        {
            writeableBitmap =
                WriteableBitmap.DecodeToWidth(new MemoryStream(this.imageBytes), maxImageWidth);
            adjustedImageBytes = writeableBitmap.EncodeToJpeg();
        }

        // this.jigsawModel.AddToCollection(this.pictureMetadata, adjustedImageBytes, thumbnailBytes);
    }

    internal void RemoveFromCollection()
    {
        this.Provider = this.Localize("Shared.NoImage");
        this.Title = string.Empty;
        this.Copyright = string.Empty;

        var canvas = this.View.Canvas;
        canvas.Children.Clear();
        if (this.pictureMetadata is not null)
        {
            // this.jigsawModel.RemoveFromCollection(this.pictureMetadata);
            this.pictureMetadata = null;
            this.imageBytes = null;
        }
    }

    internal void SaveToDesktop()
    {
        if (this.pictureMetadata is null || this.imageBytes is null)
        {
            return;
        }

        try
        {
            var fileManager = ApplicationBase.GetRequiredService<FileManagerModel>();
            fileManager.Save(
                Area.Desktop, Kind.BinaryNoExtension,
                this.pictureMetadata.TodayImageFilePath(),
                this.imageBytes);
        }
        catch (Exception ex)
        {
            string msg = "Failed to save image file: \n" + ex.ToString();
            this.Logger.Error(msg);
            var toaster = ApplicationBase.GetRequiredService<IToaster>();
            toaster.Show(
                this.Localize("Shared.FileErrorTitle"),
                this.Localize("Shared.FileErrorText"),
                10_000, InformationLevel.Warning);
        }
    }

    public void Receive(ZoomRequestMessage message)
    {
        if (message.Tag != this.parent)
        {
            return;
        }

        this.ZoomFactor = message.ZoomFactor;
    }
}
