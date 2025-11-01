namespace Lyt.Jigsaw.Workflow.Collection;

public sealed partial class CollectionViewModel : 
    ViewModel<CollectionView>,
    IRecipient<ToolbarCommandMessage>,
    IRecipient<ModelLoadedMessage>,
    IRecipient<CollectionChangedMessage>
{
    private readonly JigsawModel jigsawModel;

    [ObservableProperty]
    private ThumbnailsPanelViewModel thumbnailsPanelViewModel;

    [ObservableProperty]
    private DropViewModel dropViewModel;

    [ObservableProperty]
    private PictureViewModel pictureViewModel;

    private bool loaded;
    private List<Tuple<Picture, byte[]>>? collectionThumbnails;

    public CollectionViewModel(JigsawModel jigsawModel)
    {
        this.jigsawModel = jigsawModel;
        this.PictureViewModel = new PictureViewModel(this);
        this.DropViewModel = new DropViewModel();
        this.ThumbnailsPanelViewModel = new ThumbnailsPanelViewModel(this);
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<ModelLoadedMessage>();
        this.Subscribe<CollectionChangedMessage>();
    }

    public override void Activate(object? activationParameters) 
    {
        base.Activate(activationParameters);
        if (this.loaded)
        {
            this.UpdateSelection();
        }
    }

    public void Receive(ModelLoadedMessage _)
    {
        if (!this.loaded)
        {
            this.loaded = true;
            // TODO 
            // this.collectionThumbnails = this.jigsawModel.LoadCollectionThumbnails();
            this.ThumbnailsPanelViewModel.LoadThumnails(this.collectionThumbnails);
        }
        else
        {
            this.UpdateSelection();
        }
    }

    public void Receive(CollectionChangedMessage _)
    {
        this.loaded = false;
        this.Receive(new ModelLoadedMessage());
        this.UpdateSelection();
    }

    private void UpdateSelection()
        =>  Schedule.OnUiThread(
                200,
                () => 
                {
                    this.ThumbnailsPanelViewModel.UpdateSelection();
                    if ( this.ThumbnailsPanelViewModel.SelectedThumbnail is ThumbnailViewModel thumbnailViewModel)
                    {
                           this.Select(thumbnailViewModel.Metadata, thumbnailViewModel.ImageBytes);
                    }
                },
                DispatcherPriority.Background);

    public void Receive(ToolbarCommandMessage message) 
    {
        switch (message.Command)
        {
            case ToolbarCommandMessage.ToolbarCommand.Play:
                
                break;

            case ToolbarCommandMessage.ToolbarCommand.RemoveFromCollection:
                this.PictureViewModel.RemoveFromCollection();
                break;

            case ToolbarCommandMessage.ToolbarCommand.CollectionSaveToDesktop:
                this.PictureViewModel.SaveToDesktop();
                break;

            // Ignore all other commands 
            default:
                break;
        }
    }

    internal bool OnImageDrop(string path, byte[] imageBytes)
    {
        ApplicationMessagingExtensions.Select(ActivatedView.Puzzle);

        int decodeToWidth = 1024 + 512;
        var image =
            WriteableBitmap.DecodeToWidth(
                new MemoryStream(imageBytes), decodeToWidth, BitmapInterpolationMode.HighQuality);
        var puzzle = new Puzzle(this.Logger, image.PixelSize.Height, image.PixelSize.Width, 0);
        var counts = puzzle.PieceCounts;

        var vm = App.GetRequiredService<PuzzleViewModel>();
        // vm.Start(image, counts[counts.Count - 10], rotationSteps: 2, randomize: true);
        vm.Start(image, counts[16 /*counts.Count - 21 */ ], rotationSteps: 0, randomize: true);
        // vm.Start(image, counts[0], rotationSteps: 6);

        return true ;
    }

    internal void Select(PictureMetadata pictureMetadata, byte[] _)
    {
        // We receive the bytes of the thumbnail so we need to load the full image 
        bool showBadPicture = true;
        try
        {
            string? url = pictureMetadata.Url;
            // TODO 
            //
            //if (!string.IsNullOrEmpty(url) &&
            //    this.jigsawModel.Pictures.TryGetValue(url, out Picture? maybePicture) &&
            //    maybePicture is Picture picture)
            //{
            //    var fileManager = App.GetRequiredService<FileManagerModel>();
            //    var fileId = new FileId(FileManagerModel.Area.User, FileManagerModel.Kind.BinaryNoExtension, picture.ImageFilePath);
            //    if (fileManager.Exists(fileId))
            //    {
            //        // Consider caching some of the images ? 
            //        byte[] imageBytes = fileManager.Load<byte[]>(fileId);
            //        if ((imageBytes != null) && (imageBytes.Length > 256))
            //        {
            //            this.PictureViewModel.Select(pictureMetadata, imageBytes);
            //            showBadPicture = false;
            //        }
            //    }
            //}
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        if (showBadPicture)
        {
            this.ShowBadPicture();
        }
    }

    internal bool Select(string path, byte[] imageBytes)
    {
        // Here we receive the bytes of the image dropped in the drop zone 
        try
        {
            PictureMetadata pictureMetadata = new ()
            {
                Date = DateTime.Now.Date,
                Url = path,
            };

            this.PictureViewModel.Select(pictureMetadata, imageBytes);
            this.PictureViewModel.AddToCollection();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            this.ShowBadPicture();
        }

        return false;
    }

    private void ShowBadPicture()
    {
        this.PictureViewModel.Title = this.Localize("Collection.BadPicture") ;
        this.Logger.Warning("Collection: Bad picture!"); 
    }
}
