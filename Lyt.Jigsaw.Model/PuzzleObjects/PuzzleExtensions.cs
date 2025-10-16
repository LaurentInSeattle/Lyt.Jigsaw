namespace Lyt.Jigsaw.Model.PuzzleObjects; 

public static class PuzzleExtensions
{
    public static int ToId(this Puzzle puzzle, Piece piece) 
        => piece.Position.Row * puzzle.Columns + piece.Position.Column;

    public static int ToId(this Puzzle puzzle, int row, int column)
        => row * puzzle.Columns + column;
}

