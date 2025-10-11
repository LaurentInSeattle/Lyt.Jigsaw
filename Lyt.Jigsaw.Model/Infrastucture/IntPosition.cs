namespace Lyt.Jigsaw.Model.Infrastucture; 

public struct IntPosition(int row, int col)
{
    public int Row { get; private set; } = row;

    public int Column { get; private set; } = col;
}

