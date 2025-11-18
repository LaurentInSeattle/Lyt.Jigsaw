namespace Lyt.Jigsaw.Workflow.Collection;

using System.Net.WebSockets;

public sealed partial class CollectionViewModel :
    ViewModel<CollectionView>,
    IRecipient<ToolbarCommandMessage>,
    IRecipient<ModelLoadedMessage>,
    IRecipient<CollectionChangedMessage>
{
    private enum PlayStatus
    {
        Unprepared,
        ReadyForRestart,
        ReadyForNew,
    }

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
    private string contrastString;

    [ObservableProperty]
    private double contrastSliderValue;

    [ObservableProperty]
    private bool parametersVisible;

    [ObservableProperty]
    private bool parametersEnabled;

    private bool loaded;
    private PlayStatus state;
    private int pieceCount;
    private int rotations;
    private int snap;
    private int contrast;
    private Dictionary<int, PuzzleSetup> setups; 
    private List<int> pieceCounts;
    private byte[]? imageBytes;

    public CollectionViewModel(JigsawModel jigsawModel)
    {
        this.jigsawModel = jigsawModel;
        this.setups = []; 
        this.pieceCounts = [];
        this.DropViewModel = new DropViewModel();
        this.ThumbnailsPanelViewModel = new ThumbnailsPanelViewModel(this);
        this.state = PlayStatus.Unprepared;
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<ModelLoadedMessage>();
        this.Subscribe<CollectionChangedMessage>();

        // Setup UI 
        this.rotations = 1;
        this.snap = 0;
        this.contrast = 0;
        this.PieceCountString = string.Empty;
        this.RotationsString = string.Empty;
        this.SnapString = string.Empty;
        this.ContrastString = string.Empty;
        this.ParametersVisible = false;
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);

        // TODO : Set up properly sliders 
        this.OnRotationsSliderValueChanged(0.0);
        this.OnSnapSliderValueChanged(0);
        this.OnContrastSliderValueChanged(0);
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
            this.ThumbnailsPanelViewModel.LoadThumnails();
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
                    //this.ThumbnailsPanelViewModel.UpdateSelection();
                    //if (this.ThumbnailsPanelViewModel.SelectedThumbnail is ThumbnailViewModel thumbnailViewModel)
                    //{
                    //    this.Select(thumbnailViewModel.Metadata, thumbnailViewModel.ImageBytes);
                    //}
                },
                DispatcherPriority.Background);

    public void Receive(ToolbarCommandMessage message)
    {
        switch (message.Command)
        {
            case ToolbarCommandMessage.ToolbarCommand.Play:
                if (this.state == PlayStatus.ReadyForNew)
                {
                    this.StartNewGameFromDropppedImage();
                }
                else if (this.state == PlayStatus.ReadyForRestart)
                {
                    this.ResumeSavedGame();
                }
                // else: Not ready: do nothing 
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

    #region Game Parameters 

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
        this.rotations = (int)value;
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

    partial void OnContrastSliderValueChanged(double value)
    {
        this.contrast = (int)value;
        this.ContrastString =
            this.contrast == 0 ?
                "Heavy" :
                this.contrast == 1 ?
                    "Strong" :
                    this.contrast == 2 ?
                        "Weak" : "Normal";
    }

    #endregion Game Parameters 

    private void ResumeSavedGame()
    {
        try
        {
            if ((this.jigsawModel.Puzzle is not null) && (this.PuzzleImage is not null))
            {
                var vm = App.GetRequiredService<PuzzleViewModel>();
                vm.ResumePuzzle(this.jigsawModel.Puzzle, this.PuzzleImage);
                ApplicationMessagingExtensions.Select(ActivatedView.Puzzle);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private void StartNewGameFromDropppedImage()
    {
        if ((this.PuzzleImage is null) ||
            (this.imageBytes == null) || (this.imageBytes.Length == 0) ||
            (this.pieceCount == 0))
        {
            return;
        }

        try
        {
            var setup = this.setups[this.pieceCount]; 
            int decodeToWidthThumbnail = 360;
            var writeableBitmap =
                WriteableBitmap.DecodeToWidth(new MemoryStream(this.imageBytes), decodeToWidthThumbnail);
            byte[] thumbnailBytes = writeableBitmap.EncodeToJpeg();
            var vm = App.GetRequiredService<PuzzleViewModel>();
            vm.StartNewGame(
                this.imageBytes, thumbnailBytes, this.PuzzleImage,
                setup, this.rotations, this.snap);
            ApplicationMessagingExtensions.Select(ActivatedView.Puzzle);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    internal bool OnImageDrop(byte[] imageBytes)
    {
        this.imageBytes = imageBytes;
        int decodeToWidth = 1920; //  1024 + 512;
        var image =
            WriteableBitmap.DecodeToWidth(
                new MemoryStream(imageBytes), decodeToWidth, BitmapInterpolationMode.HighQuality);

        var imageSize = new IntSize(image.PixelSize.Height, image.PixelSize.Width);
        var maybeSetups = Puzzle.GenerateSetups(imageSize);
        var counts = (from count in maybeSetups.Keys orderby count descending select count).ToList();
        if (counts.Count < 3)
        {
            // Failed ? 
            this.state = PlayStatus.Unprepared;
            this.ParametersVisible = false;

            // TODO: Message
            return false; 
        }

        int max = counts[0];
        int min = counts[^1];
        this.setups = maybeSetups;
        this.pieceCounts = counts;

        // UI update 
        this.OnRotationsSliderValueChanged(1.0);
        this.PieceCountMin = min;
        this.PieceCountMax = max;
        this.PuzzleImage = image;
        this.ParametersVisible = true;

        this.state = PlayStatus.ReadyForNew;
        return true;
    }

    internal void ClearSelection ()
    {
        this.ParametersVisible = false;
        this.PuzzleImage = null;
        this.state = PlayStatus.Unprepared;
    }

    internal void Select(Model.GameObjects.Game game)
    {
        // TODO: Load game parameters into the UI 
        this.ParametersVisible = true;

        // Load the puzzle image regardless of completion status
        string gameKey = game.Name;
        byte[]? imageBytes = this.jigsawModel.LoadGame(gameKey);
        if (imageBytes is null)
        {
            return;
        }

        int decodeToWidth = 1920; //  1024 + 512;
        var image =
            WriteableBitmap.DecodeToWidth(
                new MemoryStream(imageBytes), decodeToWidth, BitmapInterpolationMode.HighQuality);
        this.PuzzleImage = image;

        try
        {
            if (game.IsCompleted)
            {
                // Completed game: allow to redo it with different parameters
                // Show settings 
                this.state = PlayStatus.ReadyForNew;
            }
            else
            {


                this.state = PlayStatus.ReadyForRestart;
            } 
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
