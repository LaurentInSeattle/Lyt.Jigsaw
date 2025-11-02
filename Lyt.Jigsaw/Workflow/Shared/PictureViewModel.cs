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

    private PictureMetadata? pictureMetadata;
    private byte[]? imageBytes;
    private int imageWidth;

    public PictureViewModel(ViewModel parent)
    {
        this.parent = parent;
        this.jigsawModel = ApplicationBase.GetRequiredService<JigsawModel>();
        this.Subscribe<ZoomRequestMessage>();
    }

    public override void OnViewLoaded()
    {
        base.OnViewLoaded();
    }

    internal void Select(byte[] imageBytes)
    {
        this.imageBytes = imageBytes;
        var bitmap = WriteableBitmap.Decode(new MemoryStream(imageBytes));
        this.imageWidth = (int)bitmap.Size.Width;
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
