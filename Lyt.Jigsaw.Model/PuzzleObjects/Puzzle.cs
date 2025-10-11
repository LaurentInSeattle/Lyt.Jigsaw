namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Puzzle
{
    private readonly Dictionary<int, PuzzleSetup> puzzleSetups;

    public Puzzle(int height, int width, int rotationStep)
    {
        this.ImageSize = new(height, width);
        this.RotationStep = rotationStep;
        this.puzzleSetups = [];
        this.GenerateSetups();
    }

    public IntSize ImageSize { get; set; }

    public int PieceSize { get; set; }

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public int PieceOverlap { get; private set; }

    public int RotationStep { get; private set; }

    public int PieceCount { get; private set; }

    public List<Group> Groups { get; private set; } = [];

    public bool IsComplete
        => this.Groups.Count == 1 && this.Groups[0].Pieces.Count == this.PieceCount;

    public List<int> PieceCounts => [.. this.puzzleSetups.Keys];

    public bool Setup(int pieceCount, int rotationSteps)
    {
        if ((rotationSteps < 0) || (rotationSteps > 6))
        {
            return false;
        }

        if (this.puzzleSetups.TryGetValue(pieceCount, out var setup) && setup is not null)
        {
            this.Rows = setup.Rows;
            this.Columns = setup.Columns;
            this.PieceSize = setup.PieceSize;
            this.PieceOverlap = setup.PieceSize / 4;
            this.PieceCount = pieceCount;
            this.RotationStep = 360 / rotationSteps;
            return true;
        }

        return false;
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
}
