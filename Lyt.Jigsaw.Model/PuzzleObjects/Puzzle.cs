namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Puzzle
{
    private readonly Dictionary<int, PuzzleSetup> puzzleSetups;
    internal readonly Randomizer Randomizer; 

    public Puzzle(int height, int width, int rotationSteps)
    {
        this.ImageSize = new(height, width);
        this.RotationSteps = rotationSteps;
        this.Randomizer = new Randomizer();
        this.puzzleSetups = [];
        this.GenerateSetups();
    }

    public IntSize ImageSize { get; set; }

    public int PieceSize { get; set; }

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public int PieceOverlap { get; private set; }

    public int RotationSteps { get; private set; }

    public int RotationStepAngle { get; private set; }

    public int PieceCount { get; private set; }

    public List<Group> Groups { get; private set; } = [];

    public Dictionary<int, Group> GroupDictionary { get; private set; } = [];

    public List<Piece> Pieces { get; private set; } = [];

    public Dictionary<int, Piece> PieceDictionary { get; private set; } = [];

    public bool IsComplete
        => this.Groups.Count == 1 && this.Groups[0].Pieces.Count == this.PieceCount;

    public List<int> PieceCounts => [.. this.puzzleSetups.Keys];

    public Piece FromId(int id)
        => this.PieceDictionary.TryGetValue(id, out Piece? piece) && piece is not null ?
                piece :
                throw new ArgumentException("No such Piece Id "); 

    public bool Setup(int pieceCount, int rotationSteps)
    {
        if ((rotationSteps < 0) || (rotationSteps > 6))
        {
            return false;
        }

        if (!this.puzzleSetups.TryGetValue(pieceCount, out var setup) || setup is null)
        {
            return false;
        }

        this.Rows = setup.Rows;
        this.Columns = setup.Columns;
        this.PieceSize = setup.PieceSize;
        this.PieceOverlap = setup.PieceSize / 4;
        this.PieceCount = pieceCount;
        if (rotationSteps == 0)
        {
            this.RotationStepAngle = 0;
        }
        else
        {
            this.RotationStepAngle = 360 / rotationSteps;
        }

        this.CreatePieces();
        return true;
    }

    private void GenerateSetups()
    {
        int minDimension = Math.Min(this.ImageSize.Width, this.ImageSize.Height);
        int maxPieceSize = minDimension / 4;
        for (int pieceSize = 32; pieceSize <= maxPieceSize; pieceSize += 4)
        {
            var setup = new PuzzleSetup(pieceSize, this.ImageSize);
            int pieceCount = setup.Rows * setup.Columns;
            this.puzzleSetups.TryAdd(pieceCount, setup);
        }
    }

    private void CreatePieces()
    {
        for (int row = 0; row < this.Rows; ++row)
        {
            for (int col = 0; col < this.Columns; ++col)
            {
                var piece = new Piece(this, row, col); 
                this.Pieces.Add(piece);
                this.PieceDictionary.Add(piece.Id, piece);
            }
        }

        for (int row = 0; row < this.Rows; ++row)
        {
            for (int col = 0; col < this.Columns; ++col)
            {
                var piece = this.FromId(this.ToId(row, col));
                piece.UpdateSides(); 
            }
        }

        for (int row = 0; row < this.Rows; ++row)
        {
            for (int col = 0; col < this.Columns; ++col)
            {
                var piece = this.FromId(this.ToId(row, col));
                if (piece.AnySideUnknown)
                {
                    throw new Exception("Failed to calculate sides"); 
                }
            }
        }
    }
}
