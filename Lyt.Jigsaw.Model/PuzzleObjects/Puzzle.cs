namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Puzzle
{
    private readonly Dictionary<int, PuzzleSetup> puzzleSetups;

    internal readonly Randomizer Randomizer;

    private readonly Profiler profiler;

    public Puzzle(ILogger logger, int height, int width, int rotationSteps)
    {
        this.ImageSize = new(height, width);
        this.RotationSteps = rotationSteps;
        this.Randomizer = new Randomizer();
        this.profiler = new Profiler(logger);
        this.puzzleSetups = [];
        this.PieceSnapDistance = 6.66;
        this.GenerateSetups();
    }

    public IntSize ImageSize { get; set; }

    public double PieceSnapDistance { get; private set; }

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public int PieceSize { get; set; }

    public int PieceOverlap { get; private set; }

    public int RotationSteps { get; private set; }

    public int RotationStepAngle { get; private set; }

    public int PieceCount { get; private set; }

    public List<Group> Groups { get; private set; } = [];

    public Dictionary<int, Group> GroupDictionary { get; private set; } = [];

    public List<Piece> Pieces { get; private set; } = [];

    public Dictionary<int, Piece> PieceDictionary { get; private set; } = [];

    public int ApparentPieceSize => this.PieceSize - 2 * this.PieceOverlap;

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
        int maxPieceSize = minDimension / 3;
        for (int pieceSize = 32; pieceSize <= maxPieceSize; pieceSize += 4)
        {
            var setup = new PuzzleSetup(pieceSize, this.ImageSize);
            int pieceCount = setup.Rows * setup.Columns;
            this.puzzleSetups.TryAdd(pieceCount, setup);
        }
    }

    private void CreatePieces()
    {
        this.profiler.StartTiming();

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

        // Measured at less of 12 ms in debug build for 180 pieces 
        // Measured at less of 35 ms in debug build for 1920 pieces 
        this.profiler.EndTiming(string.Format("Creating point lists - Piece Count: {0}", this.PieceCount));

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

    public void Save()
    {
        // TODO 
        // Serialize and save to disk 
        Debug.WriteLine("Saved");
    }

    public Piece? FindCloseTo(Piece targetPiece , out Placement placement)
    {
        placement = Placement.Unknown; 
        double minDistance = double.MaxValue;
        Piece? closestPiece = null;
        foreach (Piece piece in this.Pieces)
        {
            if (targetPiece == piece)
            {
                continue;
            }

            // pieces should have the same orientation 
            if (piece.RotationAngle != targetPiece.RotationAngle)
            {
                continue;
            }

            // distance should be equal to piece size + or - the snap visual distance 
            double distance = Location.Distance(piece.Center, targetPiece.Center);
            if (distance < minDistance)
            {
                minDistance = distance;
                double delta = Math.Abs(distance - this.PieceSize);
                if (delta > this.PieceSnapDistance)
                {
                    continue;
                }

                // pieces should share one side  
                if (!piece.ShareOneSideWith(targetPiece, out placement))
                {
                    continue;
                }

                closestPiece = piece;
            }
        }

        return closestPiece;
    }

    public bool CheckForMatchingPiece(Piece targetPiece)
    {
        // Find closest piece
        Piece? closest = this.FindCloseTo(targetPiece, out Placement placement);
        if (closest is null)
        {
            return false;
        }

        closest.SnapTo(targetPiece, placement);
        return true;
    }
}
