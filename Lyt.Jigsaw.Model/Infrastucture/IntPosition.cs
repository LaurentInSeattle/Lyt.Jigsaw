namespace Lyt.Jigsaw.Model.Infrastucture; 

public struct IntPosition(int row, int col)
{
    public int Row { get; set; } = row;

    public int Column { get; set; } = col;

    public readonly int ToId(Puzzle puzzle) => this.Row * puzzle.Columns + this.Column;
}

