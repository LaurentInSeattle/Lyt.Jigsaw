namespace Lyt.Jigsaw.Workflow.Game;

using Location = Model.Infrastucture.Location;

public sealed partial class PieceViewModel : ViewModel<PieceView>, IDragMovableViewModel, IRecipient<ShowEdgesMessage>
{
    [ObservableProperty]
    private CroppedBitmap croppedBitmap;

    [ObservableProperty]
    private Geometry clipGeometry;

    [ObservableProperty]
    private Transform? imageRotationTransform;

    [ObservableProperty]
    private Transform? pathRotationTransform;

    [ObservableProperty]
    private bool isHitTestVisible; 

    [ObservableProperty]
    private bool pathIsVisible;

    [ObservableProperty]
    private bool isVisible;

    private readonly JigsawModel jigsawModel;
    private readonly PuzzleViewModel puzzleViewModel;
    private readonly Piece piece;

    public PieceViewModel(JigsawModel jigsawModel, PuzzleViewModel puzzleViewModel, Piece piece)
    {
        if (puzzleViewModel.Image is null)
        {
            throw new ArgumentException("No Image");
        }

        this.puzzleViewModel = puzzleViewModel;
        this.jigsawModel = jigsawModel;
        this.piece = piece;
        var puzzle = piece.Puzzle;

        // Cropping 
        // Cropping works just fine with negative origin at top and left edges 
        int roiSize = puzzle.PieceSize + 2 * puzzle.PieceOverlap;
        int roiX = piece.Position.Column * puzzle.PieceSize - puzzle.PieceOverlap;
        int roiY = piece.Position.Row * puzzle.PieceSize - puzzle.PieceOverlap;
        var rectangleRoi = new PixelRect(roiX, roiY, roiSize, roiSize);
        var cropped = new CroppedBitmap(puzzleViewModel.Image, rectangleRoi);
        this.CroppedBitmap = cropped;

        // Clipping 
        int outerSize = puzzle.PieceSize + 2 * puzzle.PieceOverlap;
        var outerGeometry = new RectangleGeometry(new Rect(0, 0, outerSize, outerSize), 0, 0);
        double scale = outerSize / 1200.0;
        var innerGeometry =
            GeometryGenerator.Combine(
                piece.TopPoints.ToScaledPoints(scale),
                piece.RightPoints.ToScaledPoints(scale),
                piece.BottomPoints.ToScaledPoints(scale),
                piece.LeftPoints.ToScaledPoints(scale),
                IntPointList.DummyPoints.ToScaledPoints(scale));
        this.ClipGeometry = GeometryGenerator.InvertedClip(outerGeometry, innerGeometry);

        this.IsHitTestVisible = true;
        this.IsVisible = true;
        this.PathIsVisible = true;
        this.Subscribe<ShowEdgesMessage>();
    }

    public void Receive(ShowEdgesMessage message)
    {
        bool showEdges = message.Show;
        if ( ! showEdges)
        {
            this.IsVisible = true;
        }
        else
        {
            this.IsVisible = this.piece.IsGrouped || this.piece.IsEdge;
        }

        this.IsHitTestVisible = this.IsVisible;
        var puzzle = piece.Puzzle;
        if (!puzzle.IsComplete)
        {
            this.PathIsVisible = this.IsVisible;
        } 
    }

    public void OnComplete()
    {
        // Hide the path and remove any effects
        this.PathIsVisible = false;
        this.View.Image.Effect = null;
    } 

    public void OnEntered() { }

    public void OnExited() { }

    public void OnLongPress() { }

    public void OnClicked(bool isRightClick)
    {
        this.jigsawModel.RotatePuzzlePiece(this.piece, isCCW: isRightClick); 

        if (this.piece.IsGrouped)
        {
            this.puzzleViewModel.UpdateLocationsAfterSnap(); 
        }
        else
        {
            this.View.Rotate(this.piece.RotationAngle);
        }
    }

    public bool OnBeginMove(Point fromPoint)
    {
        double distance = 
            Location.Distance(new Location(fromPoint.X, fromPoint.Y), this.piece.Center);
        return distance < this.piece.Puzzle.ApparentPieceSize * 2.1;
    }

    public void OnEndMove(Point fromPoint, Point toPoint)
    {
        // No need to make sure toPoint is inside the canvas 
        this.OnMove(fromPoint, toPoint);

        // Check for any snaps
        if (this.jigsawModel.CheckForPuzzleSnaps(this.piece))
        {
            this.puzzleViewModel.UpdateLocationsAfterSnap(); 
        }
    }

    public void OnMove(Point fromPoint, Point toPoint)
    {
        // No need to make sure toPoint is inside the canvas 
        this.piece.MoveTo(toPoint.X, toPoint.Y);

        if (this.piece.IsGrouped)
        {
            // this.Profiler.StartTiming();

            foreach (Piece other in this.piece.Group.Pieces)
            {
                if (piece == other)
                {
                    continue;
                }

                // Move on the UI and bring the pieces on top 
                var pieceView = this.puzzleViewModel.GetViewFromPiece(other);
                pieceView.MoveTo(other.Location);
                pieceView.BringToTop();
            }

            // Takes about 5ms for 1000 pieces
            // this.Profiler.EndTiming("Move Group Pieces");
        }
    }
}

