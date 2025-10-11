namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Puzzle
{
    private readonly Dictionary<int, Tuple<int, int>> pieceArrays; 

    public Puzzle(int height, int width, int rotationStep)
    {
        this.Height = height;
        this.Width = width;
        this.RotationStep = rotationStep;
        this.pieceArrays = [];
        this.GeneratePieceArrays(); 
    }

    public int Height { get; private set; }

    public int Width { get; private set; }

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public int PieceOverlap { get; private set; }

    public int RotationStep { get; private set; }

    public int PieceCount { get; private set; }

    public List<Group> Groups { get; private set; } = [];

    public bool IsComplete 
        => this.Groups.Count == 1 && this.Groups[0].Pieces.Count == this.PieceCount;

    public List<int> PieceCounts => [.. this.pieceArrays.Keys];

    public bool SelectPieceCount (int pieceCount)
    {
        if (this.pieceArrays.TryGetValue(pieceCount, out var pieceArray) && pieceArray is not null)
        {
            this.Rows = pieceArray.Item1;
            this.Columns = pieceArray.Item2;
            this.PieceCount = pieceCount;
            return true;
        }

        return false;
    }

    private void GeneratePieceArrays()
    {

    }
}
