namespace Lyt.Jigsaw.Workflow.Collection;

using Lyt.Jigsaw.Model.Utilities;

public sealed partial class CollectionViewModel :
    ViewModel<CollectionView>,
    IRecipient<ToolbarCommandMessage>,
    IRecipient<ModelLoadedMessage>
{
    private const int DecodeToWidthThumbnail = 420;

    private enum PlayStatus
    {
        Unprepared,
        ReadyForRestart,
        ReadyForNew,
    }

    private readonly JigsawModel jigsawModel;

    // NOT cropped and NO contrast applied
    private WriteableBitmap? sourceImage;

    [ObservableProperty]
    private ThumbnailsPanelViewModel thumbnailsPanelViewModel;

    [ObservableProperty]
    private DropViewModel dropViewModel;

    [ObservableProperty]
    // Either copy of sourceImage and possibly cropped and with requested contrast applied
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
        this.ThumbnailsPanelViewModel = new ThumbnailsPanelViewModel(this, jigsawModel);
        this.state = PlayStatus.Unprepared;
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<ModelLoadedMessage>();

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
        this.OnContrastSliderValueChanged(4);
        this.ThumbnailsPanelViewModel.LoadThumnails();
        this.loaded = true;

        if  (this.jigsawModel.HasNoSavedGames())
        {
            this.SimulateDropImage();
        }
    }

    private void SimulateDropImage()
    {
        var randomizer = App.GetRequiredService<IRandomizer>();
        bool useFirstImage = randomizer.NextBool();
        string resourceName =
            useFirstImage ?
                "Bonheur_Matisse.jpg" :
                "ZhangDaqiang.jpg";
        ResourcesUtilities.SetResourcesPath("Lyt.Jigsaw.Resources");
        ResourcesUtilities.SetExecutingAssembly(Assembly.GetExecutingAssembly());
        byte[] imageBytes = ResourcesUtilities.LoadEmbeddedBinaryResource(resourceName, out string? _);
        this.OnImageDrop(imageBytes);
    }


    public void Receive(ModelLoadedMessage _)
    {
        if (!this.loaded)
        {
            this.ThumbnailsPanelViewModel.LoadThumnails();
            this.loaded = true;
        }
    }

    public void Receive(ToolbarCommandMessage message)
    {
        switch (message.Command)
        {
            case ToolbarCommandMessage.ToolbarCommand.Play:
                this.Play();
                break;

            case ToolbarCommandMessage.ToolbarCommand.RemoveFromCollection:
                this.DeleteGame();
                break;

            case ToolbarCommandMessage.ToolbarCommand.CollectionSaveToDesktop:
                // TODO : Implement saving puzzle image to desktop
                // this.SaveToDesktop();
                break;

            // Ignore all other commands 
            default:
                break;
        }
    }

    private void DeleteGame()
    {
        var game = this.jigsawModel.Game;
        if (game is null)
        {
            return;
        }

        if (!this.jigsawModel.DeleteGame(game.Name, out string message))
        {
            Debug.WriteLine(message);
        }

        this.ThumbnailsPanelViewModel.LoadThumnails();
    }

    private void Play()
    {
        if (this.state == PlayStatus.ReadyForNew)
        {
            this.StartNewGame();
        }
        else if (this.state == PlayStatus.ReadyForRestart)
        {
            this.ResumeSavedGame();
        }
        // else: Not ready: do nothing 
    }

    private void ResumeSavedGame()
    {
        try
        {
            if ((this.jigsawModel.Puzzle is not null) && (this.PuzzleImage is not null))
            {
                var vm = App.GetRequiredService<PuzzleViewModel>();
                vm.ResumePuzzle(this.jigsawModel.Puzzle, this.PuzzleImage);
                ActivatePuzzleView();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private void StartNewGame()
    {
        if ((this.PuzzleImage is null) ||
            (this.imageBytes == null) || (this.imageBytes.Length == 0) ||
            (this.pieceCount == 0))
        {
            return;
        }

        try
        {
            PuzzleSetup setup = this.setups[this.pieceCount];
            var writeableBitmap =
                WriteableBitmap.DecodeToWidth(new MemoryStream(this.imageBytes), DecodeToWidthThumbnail);
            byte[] thumbnailBytes = writeableBitmap.EncodeToJpeg();
            var vm = App.GetRequiredService<PuzzleViewModel>();
            vm.StartNewGame(
                this.imageBytes, thumbnailBytes, this.PuzzleImage,
                setup, this.rotations, this.snap);
            ActivatePuzzleView(); 
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private static void ActivatePuzzleView ()
    {
        ViewSelector<ActivatedView>.Enable(ActivatedView.Puzzle);
        ApplicationMessagingExtensions.Select(ActivatedView.Puzzle);
    }

    internal bool OnImageDrop(byte[] imageBytes)
    {
        this.imageBytes = imageBytes;
        int decodeToWidth = 1920; //  1024 + 512;
        var image =
            WriteableBitmap.DecodeToWidth(
                new MemoryStream(imageBytes), decodeToWidth, BitmapInterpolationMode.HighQuality);

        // Make sure height is multiple of 8 so that CLAHE works properly later
        int imageHeight = image.PixelSize.Height;
        int remain = imageHeight % 8;

        if (remain == 0)
        {
            // No cropping needed: already multiple of 8
            this.sourceImage = image;

            // image bytes are already loaded
            this.imageBytes = imageBytes;
        }
        else
        {
            // Crop equally from top and bottom
            int div8 = imageHeight >> 3;
            imageHeight = div8 * 8;
            int pixelOffset = remain / 2;
            var cropped = image.Crop(new PixelRect(0, pixelOffset, decodeToWidth, imageHeight));

            this.sourceImage = cropped;
            var temp = cropped.Duplicate();
            this.imageBytes = temp.EncodeToJpeg(); 
        }

        this.PuzzleImage = this.sourceImage;
        this.SetupUiForNewGame();

        return true;
    }

    internal void ClearSelection()
    {
        this.ParametersVisible = false;
        this.PuzzleImage = null;
        this.state = PlayStatus.Unprepared;
    }

    internal void Select(Model.GameObjects.Game game)
    {
        // TODO: Load game parameters into the UI 

        // Load the puzzle image regardless of completion status
        string gameKey = game.Name;
        byte[]? imageBytes = this.jigsawModel.LoadGame(gameKey);
        if (imageBytes is null)
        {
            return;
        }


        try
        {
            int decodeToWidth = 1920; //  1024 + 512;
            var image =
                WriteableBitmap.DecodeToWidth(
                    new MemoryStream(imageBytes), decodeToWidth, BitmapInterpolationMode.HighQuality);
            this.PuzzleImage = image;
            this.imageBytes = imageBytes;
            this.ParametersVisible = game.IsCompleted;
            if (game.IsCompleted)
            {
                // Completed game: allow to redo it with different parameters
                // Need to duplicate the image 
                this.sourceImage = image.Duplicate();

                // Show settings 
                this.SetupUiForNewGame();
            }
            else
            {
                // No need to duplicate the image 
                this.state = PlayStatus.ReadyForRestart;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
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
                "Extreme" :
                this.contrast == 1 ?
                    "Strong" :
                    this.contrast == 2 ?
                        "Medium" :
                        this.contrast == 3 ? "Weak" : "None";

        if (this.sourceImage is null || this.setups.Count == 0)
        {
            return;
        }

        if (this.contrast == 4)
        {
            // No contrast adjustment 
            this.PuzzleImage = this.sourceImage;
            return;
        }
        else
        {
            // Make sure we have a proper setup 
            if (!this.setups.TryGetValue(this.pieceCount, out PuzzleSetup? setup) || setup is null)
            {
                return;
            }

            // Apply CLAHE contrast adjustment on the source image
            float clipLimit =
                this.contrast == 0 ?
                    7.5f :
                    this.contrast == 1 ?
                        6.0f :
                        this.contrast == 2 ? 
                            4.5f :
                            this.contrast == 3 ? 3.0f : 0.0f;
            WriteableBitmap contrastAjusted = sourceImage.Clahe(clipLimit, this.Profiler);

            // Update the puzzle image
            this.PuzzleImage = contrastAjusted;

            // Update the image bytes that will be passed to the puzzle 
            var temp = contrastAjusted.Duplicate();
            this.imageBytes = temp.EncodeToJpeg();
        }
    }

    #endregion Game Parameters 

    private bool SetupUiForNewGame()
    {
        if (this.PuzzleImage is null)
        {
            return false;
        }

        var pixelSize = this.PuzzleImage.PixelSize;
        var maybeSetups = Puzzle.GenerateSetups(new IntSize(pixelSize.Height, pixelSize.Width));
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
        this.ParametersVisible = true;
        this.ParametersEnabled = true;

        this.state = PlayStatus.ReadyForNew;
        return true;
    }
}
