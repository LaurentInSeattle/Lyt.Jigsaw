namespace Lyt.Jigsaw.Workflow.Game; 

public sealed partial class PuzzleViewModel : ViewModel<PuzzleView > // , IRecipient<LanguageChangedMessage>
{
    public Puzzle? Puzzle;

    public WriteableBitmap? Image;

    [ObservableProperty]
    private double canvasWidth;

    [ObservableProperty]
    private double canvasHeight;

    public void Start (WriteableBitmap image, int pieceCount, int rotationSteps)
    {
        this.Profiler.StartTiming();

        this.Image = image;
        PixelSize imagePixelSize = image.PixelSize; 
        this.Puzzle = new Puzzle(this.Logger, imagePixelSize.Height, imagePixelSize.Width, rotationSteps);
        this.Puzzle.Setup(pieceCount, rotationSteps);
        int pieceSize = this.Puzzle.PieceSize;
        int pieceSizeWithOverlap = this.Puzzle.PieceSize + 2 * this.Puzzle.PieceOverlap;
        this.CanvasWidth = pieceSizeWithOverlap * this.Puzzle.Columns;
        this.CanvasHeight = pieceSizeWithOverlap * this.Puzzle.Rows;
        
        foreach (Piece piece in this.Puzzle.Pieces)
        {
            var vm = new PieceViewModel(this, piece);
            PieceView view = vm.CreateViewAndBind();
            view.AttachBehavior(this.View.InnerCanvas); 
            this.View.InnerCanvas.Children.Add(view);
            var position = piece.Position; 
            view.SetValue(Canvas.TopProperty, (double)position.Row * pieceSizeWithOverlap);
            view.SetValue(Canvas.LeftProperty, (double)position.Column * pieceSizeWithOverlap);
        }

        // For 1400 pieces, in DEBUG build:  *****Creating pieces - Timing: 432,5 ms.  
        this.Logger.Info(string.Format("Piece Count: {0}", this.Puzzle.PieceCount)); 
        this.Profiler.EndTiming("Creating pieces");
    }

    //[RelayCommand]
    //public void OnDoSomething()
    //{
    //}

    // public void Receive(LanguageChangedMessage message) => this.Localize();

    //private void Localize()
    //{
    //}
}

