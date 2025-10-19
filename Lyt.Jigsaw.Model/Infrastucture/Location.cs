namespace Lyt.Jigsaw.Model.Infrastucture; 

public struct Location(double x, double y)
{
    public double X { get; private set; } = x;

    public double Y { get; private set; } = y;
}