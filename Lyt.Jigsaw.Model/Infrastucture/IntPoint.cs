namespace Lyt.Jigsaw.Model.Infrastucture; 

public struct IntPoint(int x, int y)
{
    public int X { get; private set; } = x;

    public int Y { get; private set; } = y;
}

