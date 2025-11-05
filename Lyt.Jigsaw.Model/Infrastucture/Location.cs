namespace Lyt.Jigsaw.Model.Infrastucture;

public struct Location(double x, double y)
{
    public double X { get; set; } = x;

    public double Y { get; set; } = y;

    public static double Distance(Location value1, Location value2)
    {
        double dx = value2.X - value1.X;
        double dy = value2.Y - value1.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}