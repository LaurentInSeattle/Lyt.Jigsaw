namespace Lyt.Jigsaw.Model.Infrastucture;

public struct Location(double x, double y)
{
    public double X { get; private set; } = x;

    public double Y { get; private set; } = y;

    public static double Distance(Location value1, Location value2)
    {
        double distanceSquared =
            ((value2.X - value1.X) * (value2.X - value1.X)) +
            ((value2.Y - value1.Y) * (value2.Y - value1.Y));
        return Math.Sqrt(distanceSquared);
    }
}