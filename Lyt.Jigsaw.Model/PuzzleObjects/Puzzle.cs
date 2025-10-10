namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Puzzle
{
    public int PieceOverlap { get; set; }

    public int RotationStep { get; set; }

    public int PieceCount { get; set; }

    public List<Group> Groups { get; set; } = []; 

    public bool IsComplete { get; }
}
