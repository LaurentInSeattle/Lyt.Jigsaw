namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Group
{
    private readonly Puzzle puzzle;

    public Group (Puzzle puzzle)
    {
        this.puzzle = puzzle;
    }

    public List<Piece> Pieces { get; set; } = [];

    public Location Location { get; set; }

    public bool CanAddPiece (Piece piece)
    {
        return false;
    }

    public void AddPiece(Piece piece)
    {
    }
}
