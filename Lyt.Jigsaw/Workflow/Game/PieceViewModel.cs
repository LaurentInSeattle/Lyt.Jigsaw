namespace Lyt.Jigsaw.Workflow.Game;

public sealed partial class PieceViewModel : ViewModel<PieceView>, IDragMovableViewModel
{
    [ObservableProperty]
    private CroppedBitmap croppedBitmap;

    [ObservableProperty]
    private Geometry clipGeometry;

    [ObservableProperty]
    private Transform rotationTransform;

    [ObservableProperty]
    private bool pathIsVisible;

    private readonly PuzzleViewModel puzzleViewModel;

    private readonly Piece piece;

    public PieceViewModel(PuzzleViewModel puzzleViewModel, Piece piece)
    {
        if (puzzleViewModel.Image is null)
        {
            throw new ArgumentException("No Image");
        }

        this.puzzleViewModel = puzzleViewModel;
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

        this.RotationTransform = new RotateTransform(this.piece.RotationAngle);
        this.PathIsVisible = true; 
    }

    public void OnComplete ()
        => this.PathIsVisible = false;

    public void OnEntered() { }

    public void OnExited() { }

    public void OnLongPress() { }

    public void OnClicked(bool isRightClick)
    {
        if (this.piece.IsGrouped)
        {
            this.piece.Group.Rotate(this.piece, isCCW: isRightClick);
            this.puzzleViewModel.Update(); 
        }
        else
        {
            this.piece.Rotate(isCCW: isRightClick);
            this.RotationTransform = new RotateTransform(this.piece.RotationAngle);
        }
    }

    public bool OnBeginMove(Point fromPoint)
    {
        return true;
        // double distance = Point.Distance(fromPoint, this.piece.Center.ToPoint()); 
        // return distance < this.piece.Puzzle.ApparentPieceSize;
    }

    public void OnEndMove(Point fromPoint, Point toPoint)
    {
        this.OnMove(fromPoint, toPoint);

        // Check for any snaps
        var puzzle = this.piece.Puzzle; 
        if (puzzle.CheckForSnaps(this.piece))
        {
            this.puzzleViewModel.Update(); 
        }
    }

    public void OnMove(Point fromPoint, Point toPoint)
    {
        this.piece.MoveTo(toPoint.X, toPoint.Y);

        if (this.piece.IsGrouped)
        {
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
        }
    }
}

