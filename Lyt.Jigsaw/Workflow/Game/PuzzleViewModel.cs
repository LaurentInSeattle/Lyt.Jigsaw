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
        Debugger.Break(); 

        this.Image = image;
        PixelSize imagePixelSize = image.PixelSize; 
        this.Puzzle = new Puzzle(this.Logger, imagePixelSize.Height, imagePixelSize.Width, rotationSteps);
        this.CanvasWidth = imagePixelSize.Width + 4 * this.Puzzle.PieceSize;
        this.CanvasHeight = imagePixelSize.Height + 2 * this.Puzzle.PieceSize;
        Puzzle.Setup(pieceCount, rotationSteps);
        foreach (Piece piece in this.Puzzle.Pieces)
        {
            var vm = new PieceViewModel(this, piece);
            var view = vm.CreateViewAndBind(); 
            this.View.InnerCanvas.Children.Add(view);
            var position = piece.Position; 
            view.SetValue(Canvas.TopProperty, (double)position.Row * this.Puzzle.PieceSize);
            view.SetValue(Canvas.LeftProperty, (double)position.Column * this.Puzzle.PieceSize);
        }
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

