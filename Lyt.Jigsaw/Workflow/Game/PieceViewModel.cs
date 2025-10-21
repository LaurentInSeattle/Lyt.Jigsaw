namespace Lyt.Jigsaw.Workflow.Game;

using Location = Model.Infrastucture.Location;

public sealed partial class PieceViewModel : ViewModel<PieceView> , IDragMovableViewModel
{
    [ObservableProperty]
    private CroppedBitmap croppedBitmap;

    [ObservableProperty]
    private Geometry clipGeometry;

    [ObservableProperty]
    private Transform rotationTransform;

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
        int roiSize = puzzle.PieceSize + 2 * puzzle.PieceOverlap;
        int roiX = piece.Position.Column * puzzle.PieceSize - puzzle.PieceOverlap;
        roiX = Math.Max(0, roiX);
        int roiY = piece.Position.Row * puzzle.PieceSize - puzzle.PieceOverlap;
        roiY = Math.Max(0, roiY);
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
    }

    public void OnEntered() { }

    public void OnExited() { }
    
    public void OnLongPress() { }
    
    public void OnClicked(bool isRightClick)
    {
        this.piece.Rotate(isCCW: isRightClick);
        this.RotationTransform = new RotateTransform(this.piece.RotationAngle);
    }

    public bool OnBeginMove(Point fromPoint)
    {
        return true; 
        // double distance = Point.Distance(fromPoint, this.piece.Center.ToPoint()); 
        // return distance < this.piece.Puzzle.ApparentPieceSize;
    }   

    public void OnEndMove(Point fromPoint, Point toPoint)
    {
        this.piece.MoveTo(toPoint.X, toPoint.Y);

        // Check for any match 
        if ( this.piece.Puzzle.CheckForMatchingPiece(this.piece))
        {
            // Snap on the UI 
            this.View.MoveTo(this.piece.Location);
        }
    }

    public void OnMove(Point fromPoint, Point toPoint)
    {
        this.piece.MoveTo(toPoint.X, toPoint.Y);
    }


    //[RelayCommand]
    //public void OnDoSomething()
    //{
    //}
}

