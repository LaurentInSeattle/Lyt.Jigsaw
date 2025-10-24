namespace Lyt.Jigsaw.Workflow.Game;

public sealed partial class PuzzleViewModel : ViewModel<PuzzleView> // , IRecipient<LanguageChangedMessage>
{
    public Puzzle? Puzzle;

    public WriteableBitmap? Image;

    [ObservableProperty]
    private double canvasWidth;

    [ObservableProperty]
    private double canvasHeight;

    private Dictionary<Piece, PieceViewModel>? pieceViewModels;

    public void Start(WriteableBitmap image, int pieceCount, int rotationSteps)
    {
        this.Profiler.StartTiming();

        this.pieceViewModels = [];
        this.Image = image;
        PixelSize imagePixelSize = image.PixelSize;
        this.Puzzle = new Puzzle(this.Logger, imagePixelSize.Height, imagePixelSize.Width, rotationSteps);
        this.Puzzle.Setup(pieceCount, rotationSteps);
        int pieceSize = this.Puzzle.PieceSize;
        int pieceSizeWithOverlap = pieceSize + 2 * this.Puzzle.PieceOverlap;
        this.CanvasWidth = pieceSizeWithOverlap * this.Puzzle.Columns;
        this.CanvasHeight = pieceSizeWithOverlap * this.Puzzle.Rows;

        foreach (Piece piece in this.Puzzle.Pieces)
        {
            var vm = new PieceViewModel(this, piece);
            this.pieceViewModels.Add(piece, vm);
            PieceView view = vm.CreateViewAndBind();
            view.AttachBehavior(this.View.InnerCanvas);
            this.View.InnerCanvas.Children.Add(view);
            var position = piece.Position;
            double x = (double)position.Column * pieceSizeWithOverlap;
            double y = (double)position.Row * pieceSizeWithOverlap;
            view.SetValue(Canvas.TopProperty, y);
            view.SetValue(Canvas.LeftProperty, x);
            piece.MoveTo(x, y);
        }

        this.Puzzle.Save();

        // For 1400 pieces, in DEBUG build:  *****Creating pieces - Timing: 432,5 ms.  
        this.Logger.Info(string.Format("Piece Count: {0}", this.Puzzle.PieceCount));
        this.Profiler.EndTiming("Creating pieces");
    }

    public PieceView GetViewFromPiece(Piece piece)
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

    public PieceViewModel GetViewModelFromPiece(Piece piece)
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

    internal void Update()
    {
        if (this.Puzzle is null )
        {
            throw new Exception("Puzzle is null ");
        }

        List<Piece> movedPieces = this.Puzzle.GetMoves();
        foreach (Piece piece in movedPieces)
        {
            var pieceViewModel = this.GetViewModelFromPiece(piece);
            pieceViewModel.View.MoveTo(piece.Location);
            pieceViewModel.RotationTransform = new RotateTransform(piece.RotationAngle);
        }

        if ( this.Puzzle.IsComplete)
        {
            if (this.pieceViewModels is not null)
            {
                foreach (PieceViewModel vm in this.pieceViewModels.Values)
                {
                    vm.OnComplete();
                }
            } 
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

