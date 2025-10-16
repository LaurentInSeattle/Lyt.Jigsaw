namespace Lyt.Jigsaw.Workflow.Game; 

public sealed partial class PieceViewModel : ViewModel<PieceView > 
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
        int roiSize = puzzle.PieceSize + 2 * puzzle.PieceOverlap;
        int roiX = piece.Position.Column * puzzle.PieceSize - puzzle.PieceOverlap;
        roiX = Math.Max(0, roiX);
        int roiY = piece.Position.Row * puzzle.PieceSize - puzzle.PieceOverlap;
        roiY = Math.Max(0, roiY);
        var rectangleRoi = new PixelRect(roiX, roiY, roiSize, roiSize);
        var cropped = new CroppedBitmap(puzzleViewModel.Image, rectangleRoi);
        this.CroppedBitmap = cropped;
        this.RotationTransform = new RotateTransform(piece.RotationAngle);
    }

    //[RelayCommand]
    //public void OnDoSomething()
    //{
    //}
}

