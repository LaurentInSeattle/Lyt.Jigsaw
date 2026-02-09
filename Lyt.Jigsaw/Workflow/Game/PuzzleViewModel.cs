namespace Lyt.Jigsaw.Workflow.Game;

public sealed partial class PuzzleViewModel : ViewModel<PuzzleView>,
    IRecipient<ZoomRequestMessage>,
    IRecipient<ShowPuzzleImageMessage>,
    IRecipient<ToolbarCommandMessage>,
    IRecipient<PuzzleChangedMessage>
{
    private readonly JigsawModel jigsawModel;

    public WriteableBitmap? Image;

    [ObservableProperty]
    private double canvasWidth;

    [ObservableProperty]
    private double canvasHeight;

    [ObservableProperty]
    private double zoomFactor;

    [ObservableProperty]
    private double backgroundOpacity;

    [ObservableProperty]
    private SolidColorBrush backgroundBrush;

    [ObservableProperty]
    private WriteableBitmap? puzzleImage;

    [ObservableProperty]
    private bool puzzleImageIsVisible;

    private readonly Dictionary<Piece, PieceViewModel> pieceViewModels;

    private double savedZoomFactor;

    public PuzzleViewModel(JigsawModel jigsawModel)
    {
        this.jigsawModel = jigsawModel;
        this.savedZoomFactor = 1.0; 

        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<ZoomRequestMessage>();
        this.Subscribe<ShowPuzzleImageMessage>();
        this.Subscribe<PuzzleChangedMessage>();
        this.pieceViewModels = [];
        this.backgroundBrush = new SolidColorBrush(Colors.Transparent);
    }

    ~PuzzleViewModel()
    {
        this.Unregister<ZoomRequestMessage>();
        this.Unregister<PuzzleChangedMessage>();
    }

    public override void Activate(object? _)
        => this.jigsawModel.GameIsActive(isActive: true);

    public override void Deactivate()
    {
        // Force a full save on deactivation
        this.jigsawModel.PausePlaying();
        this.jigsawModel.SavePuzzle();
        this.jigsawModel.SaveGame();
        this.jigsawModel.GameIsActive(isActive: false);
    }

    public void Receive(ToolbarCommandMessage message)
    {
        if ((message.Command == ToolbarCommandMessage.ToolbarCommand.PlayFullscreen) ||
            (message.Command == ToolbarCommandMessage.ToolbarCommand.PlayWindowed))
        {
            if (savedZoomFactor != 1.0)
            {
                // Need to wait a bit or else the layout manager will throw 
                Schedule.OnUiThread(100, () =>
                {
                    // ensure property changed 
                    this.ZoomFactor = 1.0;
                    this.ZoomFactor = this.savedZoomFactor;
                }, DispatcherPriority.ApplicationIdle);
            } 
        }
    }

    public void Receive(ZoomRequestMessage message)
    {
        double zoomFactor =message.ZoomFactor;
        this.ZoomFactor = zoomFactor;
        this.savedZoomFactor =zoomFactor;
    }

    public void Receive(ShowPuzzleImageMessage message)
        => this.PuzzleImageIsVisible = message.Show;

    public void Receive(PuzzleChangedMessage message)
    {
        switch (message.Change)
        {
            default:
            case PuzzleChange.None:
                return;

            case PuzzleChange.Background:
                this.UpdateBackground(message.Parameter);
                break;

            case PuzzleChange.Hint:
                // display Hint, need to also move pieces on top of others
                this.UpdateLocationsAfterSnap(withZIndex: true);
                break;
        }
    }

    internal void ResumePuzzle(Puzzle puzzle, WriteableBitmap image)
    {
        this.SetupView(image);
        _ = this.SetupCanvas(puzzle);

        foreach (Piece piece in puzzle.Pieces)
        {
            var view = this.CreatePieceView(piece);
            view.MoveToAndRotate(piece.Location, piece.RotationAngle, bringToTop: false);
        }

        this.UpdateToolbarAndGameState();
    }

    public void StartNewGame(
        byte[] imageBytes, byte[] thumbnailBytes, WriteableBitmap image,
        PuzzleImageSetup setup,
        PuzzleParameters puzzleParameters,
        bool randomize = true)
    {
        PixelSize imagePixelSize = image.PixelSize;
        var game = this.jigsawModel.NewGame(
            imageBytes, thumbnailBytes,
            imagePixelSize.Height, imagePixelSize.Width,
            setup, puzzleParameters);
        if (game is null)
        {
            this.Logger.Info("Failed Creating new game");
            return;
        }

        this.Profiler.StartTiming();

        this.SetupView(image);
        var puzzle = game.Puzzle;
        int pieceSizeWithOverlap = this.SetupCanvas(puzzle);
        double pieceDistance = pieceSizeWithOverlap * 0.78;

        double xOffset;
        double yOffset;

        int pieceCount = setup.PieceCount;
        if (randomize)
        {
            int canvasRows = 2 + puzzle.Rows;
            int canvasColumns = 2 + puzzle.Columns;
            xOffset = -pieceDistance / 12.0;
            yOffset = -pieceDistance / 12.0;

            // Duplicate the list and shuffle the copy 
            var pieces = puzzle.Pieces.Shuffle().ToList();
            int pieceIndex = 0;

            void CreateAndPlacePiece(int canvasRow, int canvasCol)
            {
                if (pieceIndex < pieceCount)
                {
                    Piece piece = pieces[pieceIndex];
                    var view = this.CreatePieceView(piece);
                    double x = canvasCol * pieceDistance;
                    double y = canvasRow * pieceDistance;
                    piece.MoveTo(x + xOffset, y + yOffset);
                    view.MoveToAndRotate(piece.Location, piece.RotationAngle, bringToTop: false);

                    // Debug.WriteLine("Placed at row {0} - col {1}", canvasRow, canvasCol);

                    pieceIndex++;
                }
                // else: we're done 
            }

            void RectangularPlacement(int topRow, int rightColumn, int bottomRow, int leftColumn)
            {
                int columnCount = 1 + rightColumn - leftColumn;
                int halfColumnCount = columnCount / 2;
                bool oddColumn = columnCount - 2 * halfColumnCount > 0;

                // Top Row
                for (int count = 0; count < halfColumnCount; ++count)
                {
                    CreateAndPlacePiece(topRow, leftColumn + count);
                    CreateAndPlacePiece(topRow, rightColumn - count);
                }

                // add middle if needed 
                if (oddColumn)
                {
                    CreateAndPlacePiece(topRow, leftColumn + halfColumnCount);
                }

                // middle rows 
                for (int row = 1 + topRow; row < bottomRow; row++)
                {
                    CreateAndPlacePiece(row, leftColumn);
                    CreateAndPlacePiece(row, rightColumn);
                }

                // Bottom Row
                for (int count = 0; count < halfColumnCount; ++count)
                {
                    CreateAndPlacePiece(bottomRow, leftColumn + count);
                    CreateAndPlacePiece(bottomRow, rightColumn - count);
                }

                // add middle if needed 
                if (oddColumn)
                {
                    CreateAndPlacePiece(bottomRow, leftColumn + halfColumnCount);
                }
            }

            int top = 0;
            int right = canvasColumns - 1;
            int bottom = canvasRows - 1;
            int left = 0;

            while (bottom > top)
            {
                RectangularPlacement(top, right, bottom, left);
                if (pieceIndex >= pieceCount)
                {
                    break;
                }

                ++top;
                --bottom;
                ++left;
                --right;
            }

            if (pieceIndex < pieceCount)
            {
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        }
        else
        {
            xOffset = pieceSizeWithOverlap / 2.0;
            yOffset = pieceSizeWithOverlap;

            foreach (Piece piece in puzzle.Pieces)
            {
                var view = this.CreatePieceView(piece);
                var position = piece.Position;
                double x = (double)position.Column * pieceSizeWithOverlap;
                double y = (double)position.Row * pieceSizeWithOverlap;
                piece.MoveTo(x + xOffset, y + yOffset);
                view.MoveToAndRotate(piece.Location, piece.RotationAngle, bringToTop: false);
            }
        }

        this.jigsawModel.SavePuzzle();

        // For 1400 pieces, in DEBUG build:  *****Creating pieces - Timing: 432,5 ms.  
        this.Logger.Info(string.Format("Piece Count: {0}", puzzle.PieceCount));
        this.Profiler.EndTiming("Creating pieces");

        this.UpdateToolbarAndGameState();
    }

    private PieceView CreatePieceView(Piece piece)
    {
        var vm = new PieceViewModel(this.jigsawModel, this, piece);
        this.pieceViewModels.Add(piece, vm);
        PieceView view = vm.CreateViewAndBind();
        view.AttachBehavior(this.View.InnerCanvas);
        this.View.InnerCanvas.Children.Add(view);
        return view;
    }

    private int SetupCanvas(Puzzle puzzle)
    {
        int pieceSize = puzzle.PieceSize;
        int pieceSizeWithOverlap = pieceSize + 2 * puzzle.PieceOverlap;
        double pieceDistance = pieceSizeWithOverlap * 0.78;
        this.CanvasWidth = 1.10 * pieceDistance * (1 + puzzle.Columns);
        this.CanvasHeight = 1.10 * pieceDistance * (1 + puzzle.Rows);
        return pieceSizeWithOverlap;
    }

    private void SetupView(WriteableBitmap image)
    {
        this.View.InnerCanvas.Children.Clear();
        this.pieceViewModels.Clear();
        this.Image = image;
        this.PuzzleImage = image;
        this.PuzzleImageIsVisible = false;
    }

    private void UpdateToolbarAndGameState()
    {
        this.View.InnerCanvas.InitializeBuckets();
        this.jigsawModel.GameIsActive();
        this.jigsawModel.ResumePlaying();
        Schedule.OnUiThread(50, () =>
        {
            new PuzzleChangedMessage(PuzzleChange.Start).Publish();
            new PuzzleChangedMessage(PuzzleChange.Progress, this.jigsawModel.GetPuzzleProgress()).Publish();
        }, DispatcherPriority.Background);

    }

    internal PieceView GetViewFromPiece(Piece piece)
    {
        if (this.pieceViewModels is null || this.pieceViewModels.Count == 0)
        {
            throw new Exception("pieceViewModels is null or empty");
        }

        if (this.pieceViewModels.TryGetValue(piece, out PieceViewModel? vm) && (vm is not null))
        {
            if (vm.View is not null)
            {
                return vm.View;
            }
        }

        throw new Exception("pieceViewModels has no view for this piece.");
    }

    internal PieceViewModel GetViewModelFromPiece(Piece piece)
    {
        if (this.pieceViewModels is null || this.pieceViewModels.Count == 0)
        {
            throw new Exception("pieceViewModels is null or empty");
        }

        if (this.pieceViewModels.TryGetValue(piece, out PieceViewModel? vm) && (vm is not null))
        {
            return vm;
        }

        throw new Exception("pieceViewModels has no view model for this piece.");
    }

    internal void UpdateLocationsAfterSnap(bool withZIndex = false)
    {
        List<Piece> movedPieces = this.jigsawModel.GetPuzzleMoves();
        foreach (Piece piece in movedPieces)
        {
            var pieceViewModel = this.GetViewModelFromPiece(piece);
            var pieceView = pieceViewModel.View;
            pieceView.MoveToAndRotate(piece.Location, piece.RotationAngle, bringToTop: withZIndex);
        }

        if (this.jigsawModel.IsPuzzleComplete())
        {
            if (this.pieceViewModels is not null)
            {
                foreach (PieceViewModel vm in this.pieceViewModels.Values)
                {
                    vm.OnComplete();
                }
            }

            var collectionVm = App.GetRequiredService<CollectionViewModel>();
            collectionVm.OnPuzzleCompleted();
        }
    }

    private void UpdateBackground(double background)
    {
        if (background < 0)
        {
            background = 0;
        }

        if (background > 1.0)
        {
            background = 1.0;
        }

        byte gray = (byte)(background * 255);
        var brush = new SolidColorBrush(new Color(gray, gray, gray, gray));
        this.BackgroundBrush = brush;
    }

    // public void Receive(LanguageChangedMessage message) => this.Localize();

    //private void Localize()
    //{
    //}
}
