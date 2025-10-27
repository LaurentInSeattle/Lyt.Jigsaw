namespace Lyt.Jigsaw.Workflow.Game;

using Lyt.Jigsaw.Model.PuzzleObjects;
using System.IO.Pipelines;

public sealed partial class PuzzleViewModel : ViewModel<PuzzleView>, IRecipient<ZoomRequestMessage>
{
    public Puzzle? Puzzle;

    public WriteableBitmap? Image;

    [ObservableProperty]
    private double canvasWidth;

    [ObservableProperty]
    private double canvasHeight;

    [ObservableProperty]
    private double zoomFactor;

    private readonly Dictionary<Piece, PieceViewModel> pieceViewModels;

    public PuzzleViewModel()
    {
        this.Subscribe<ZoomRequestMessage>();
        this.pieceViewModels = [];
    }

    public void Receive(ZoomRequestMessage message)
        => this.ZoomFactor = message.ZoomFactor;

    public void Start(WriteableBitmap image, int pieceCount, int rotationSteps, bool randomize = true)
    {
        this.Profiler.StartTiming();

        this.pieceViewModels.Clear();
        this.Image = image;
        PixelSize imagePixelSize = image.PixelSize;
        this.Puzzle = new Puzzle(this.Logger, imagePixelSize.Height, imagePixelSize.Width, rotationSteps);
        this.Puzzle.Setup(pieceCount, rotationSteps);
        int pieceSize = this.Puzzle.PieceSize;
        int pieceSizeWithOverlap = pieceSize + 2 * this.Puzzle.PieceOverlap;
        int canvasRows = 1 + this.Puzzle.Rows;
        int canvasColumns = 3 + this.Puzzle.Columns;
        this.CanvasWidth = pieceSizeWithOverlap * canvasColumns;
        this.CanvasHeight = pieceSizeWithOverlap * canvasRows;


        PieceView CreatePieceView(Piece piece)
        {
            var vm = new PieceViewModel(this, piece);
            this.pieceViewModels.Add(piece, vm);
            PieceView view = vm.CreateViewAndBind();
            view.AttachBehavior(this.View.InnerCanvas);
            this.View.InnerCanvas.Children.Add(view);
            return view;
        }

        if (randomize)
        {
            // Recalculate to take into account the reduced spacing 
            double pieceDistance = pieceSizeWithOverlap * 0.88;
            canvasRows = (int)(this.CanvasHeight / pieceDistance);
            canvasColumns = (int)(this.CanvasWidth / pieceDistance);

            // Fugure out how many full canvas rows would be needed 
            int fullRows = pieceCount / canvasColumns;
            int topRows = fullRows / 2;
            int bottomRows = fullRows - topRows;
            int piecesLeft = pieceCount - fullRows * canvasColumns;
            int partialRows = canvasRows - fullRows;
            int rightPieces = piecesLeft / partialRows;
            piecesLeft = piecesLeft - rightPieces * partialRows;
            bool hasLeftPieces = piecesLeft > 0;

            // Duplicate the list and shuffle the copy 
            var pieces = this.Puzzle.Pieces.Shuffle().ToList();
            int pieceIndex = 0;
            int canvasRow = 0;
            int canvasCol = 0;
            for (int row = 0; row < this.Puzzle.Rows; row++)
            {
                for (int col = 0; col < this.Puzzle.Columns; col++)
                {

                    Piece piece = pieces[pieceIndex];
                    var view = CreatePieceView(piece);
                    double x = canvasCol * pieceDistance;
                    double y = canvasRow * pieceDistance;
                    piece.MoveTo(x, y);
                    view.MovePieceToLocation(piece);

                    ++pieceIndex;
                    ++canvasCol;
                    if (canvasCol == 1)
                    {
                        bool isMidddleRow = (canvasRow >= topRows) && (canvasRow < canvasRows - bottomRows);
                        if (isMidddleRow)
                        {
                            canvasCol = canvasColumns - rightPieces;
                        }
                    }
                    else if (canvasCol == canvasColumns)
                    {
                        canvasCol = 0;
                        ++canvasRow;
                    }
                }
            }
        }
        else
        {
            double xOffset = pieceSizeWithOverlap / 2.0;
            double yOffset = pieceSizeWithOverlap;

            foreach (Piece piece in this.Puzzle.Pieces)
            {
                var view = CreatePieceView(piece);
                var position = piece.Position;
                double x = (double)position.Column * pieceSizeWithOverlap;
                double y = (double)position.Row * pieceSizeWithOverlap;
                piece.MoveTo(x + xOffset, y + yOffset);
                view.MovePieceToLocation(piece);
            }
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
        if (this.Puzzle is null)
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

        if (this.Puzzle.IsComplete)
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

