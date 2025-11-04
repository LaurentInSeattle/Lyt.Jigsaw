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
    private WriteableBitmap? puzzleImage;

    [ObservableProperty]
    private string pieceCountString;

    [ObservableProperty]
    private int pieceCountMin;

    [ObservableProperty]
    private int pieceCountMax;

    [ObservableProperty]
    private double pieceCountSliderValue;

    [ObservableProperty]
    private string rotationsString;

    [ObservableProperty]
    private double rotationsSliderValue;

    [ObservableProperty]
    private string snapString;

    [ObservableProperty]
    private double snapSliderValue;

    [ObservableProperty]
    private bool parametersVisible;


    private bool loaded;
    private int pieceCount;
    private int rotations;
    private int snap;
    private List<int> pieceCounts;

    // TODO 
    private List<Tuple<Picture, byte[]>>? collectionThumbnails;

    public CollectionViewModel(JigsawModel jigsawModel)
    {
        this.jigsawModel = jigsawModel;
        this.pieceCounts = [];
        this.DropViewModel = new DropViewModel();
        this.ThumbnailsPanelViewModel = new ThumbnailsPanelViewModel(this);
        this.rotations = 1;
        this.snap = 0;
        this.PieceCountString = string.Empty;
        this.RotationsString = string.Empty;
        this.SnapString = string.Empty;
        this.ParametersVisible = false;
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<ModelLoadedMessage>();
        this.Subscribe<CollectionChangedMessage>();
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);
        this.OnRotationsSliderValueChanged(0.0);
        this.OnSnapSliderValueChanged(0);
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
        => Schedule.OnUiThread(
                200,
                () =>
                {
                    this.ThumbnailsPanelViewModel.UpdateSelection();
                    if (this.ThumbnailsPanelViewModel.SelectedThumbnail is ThumbnailViewModel thumbnailViewModel)
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
                this.StartGame();
                break;

            case ToolbarCommandMessage.ToolbarCommand.RemoveFromCollection:
                // this.PictureViewModel.RemoveFromCollection();
                break;

            case ToolbarCommandMessage.ToolbarCommand.CollectionSaveToDesktop:
                // this.PictureViewModel.SaveToDesktop();
                break;

            // Ignore all other commands 
            default:
                break;
        }
    }

    private void StartGame()
    {
        if ((this.PuzzleImage is null) || (this.pieceCount == 0))
        {
            return;
        }

        var vm = App.GetRequiredService<PuzzleViewModel>();
        vm.Start(this.PuzzleImage, this.pieceCount, this.rotations, this.snap);
        ApplicationMessagingExtensions.Select(ActivatedView.Puzzle);
    }

    partial void OnPieceCountSliderValueChanged(double value)
    {
        if (this.pieceCounts.Count == 0)
        {
            return;
        }

        int closest = 0;
        double minDistance = double.MaxValue;
        foreach (int count in this.pieceCounts)
        {
            double distance = Math.Abs(value - count);
            if (distance < minDistance)
            {
                closest = count;
                minDistance = distance;
            }
        }

        this.pieceCount = closest;
        this.PieceCountString = string.Format("{0:D}", this.pieceCount);
    }

    partial void OnRotationsSliderValueChanged(double value)
    {
        this.rotations = (int)value ;
        this.RotationsString =
            this.rotations <= 1 ? 
                "None" :
                string.Format("{0:D}", this.rotations);
    }

    partial void OnSnapSliderValueChanged(double value)
    {
        this.snap = (int)value;
        this.SnapString =
            this.snap == 0 ?
                "Mighty" :
                this.snap == 1 ?
                    "Strong" :
                    this.snap == 2 ?
                        "Normal" : "Weak";
    }

    internal bool OnImageDrop(string path, byte[] imageBytes)
    {
        int decodeToWidth = 1920; //  1024 + 512;
        var image =
            WriteableBitmap.DecodeToWidth(
                new MemoryStream(imageBytes), decodeToWidth, BitmapInterpolationMode.HighQuality);

        this.PuzzleImage = image;

        var puzzle = new Puzzle(this.Logger, image.PixelSize.Height, image.PixelSize.Width, 0);
        var counts = puzzle.PieceCounts;
        int max = counts[0];
        int min = counts[^1];
        this.pieceCounts = counts;
        this.PieceCountMin = min;
        this.PieceCountMax = max;
        this.OnRotationsSliderValueChanged(1.0); 
        this.ParametersVisible = true;

        return true;
    }

    internal void Select(PictureMetadata pictureMetadata, byte[] _)
    {
        // We receive the bytes of the thumbnail so we need to load the full image 
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
    }

    internal bool Select(string path, byte[] imageBytes)
    {
        // Here we receive the bytes of the image dropped in the drop zone 
        try
        {
            PictureMetadata pictureMetadata = new()
            {
                Date = DateTime.Now.Date,
                Url = path,
            };

            // this.PictureViewModel.AddToCollection();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        return false;
    }
}
