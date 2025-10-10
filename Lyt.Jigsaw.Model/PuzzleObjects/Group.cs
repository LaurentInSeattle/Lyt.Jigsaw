namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Group
{
    public List<Piece> Pieces { get; set; } = [];

    public double X { get; set; }

    public double Y { get; set; }

    public bool CanAddPiece (Piece piece)
    {
        return false;
    }

    public void AddPiece(Piece piece)
    {
    }
}
