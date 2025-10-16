namespace Lyt.Jigsaw.Model.Infrastucture;

public sealed class IntPointList : List<IntPoint> , IList<IntPoint>
{
    private static readonly Randomizer randomizer;

    static IntPointList() => IntPointList.randomizer = new Randomizer();

    public static readonly List<IntPoint> HorizontalBasePoints =
    [
        // Base
        new (0, 0),
        new (300, 20),
        new (350, -120),
        new (450, -120),
        new (500, 20),
        new (800, 0),
    ];

    public static readonly IntPointList FlatPoints =
    [
        new (0, 0),
        new (800, 0),
    ];

    public static readonly IntPointList DummyPoints =
    [
        new (0, 0),
        new (0, 0),
    ];

    public static IntPointList RandomizeBasePoints()
    {
        IntPointList points = [];
        IntPoint p0 = HorizontalBasePoints[0];
        points.Add(new IntPoint(p0.X, p0.Y));

        IntPoint p1 = HorizontalBasePoints[1];
        bool randX1 = randomizer.NextBool();
        int x1 = randX1 ? p1.X + randomizer.Next(-30, 20) : p1.X  ;  
        bool randY1 = randomizer.NextBool();
        int y1 = randY1 ? p1.Y + randomizer.Next(10, 50) : p1.Y;
        points.Add(new IntPoint(x1,y1));

        IntPoint p2 = HorizontalBasePoints[2];
        bool randX2 = randomizer.NextBool();
        int x2 = randX2 ? p2.X + randomizer.Next(-30, 30) : p2.X;
        bool randY2 = randomizer.NextBool();
        int y2 = randY2 ? p2.Y + randomizer.Next(-50, 10) : p2.Y;
        points.Add(new IntPoint(x2, y2));

        IntPoint p3 = HorizontalBasePoints[3];
        bool randX3 = randomizer.NextBool();
        int x3 = randX3 ? p3.X + randomizer.Next(-30, 30) : p3.X;
        bool randY3 = randomizer.NextBool();
        int y3 = randY3 ? p3.Y + randomizer.Next(-50, 10) : p3.Y;
        points.Add(new IntPoint(x3, y3));

        IntPoint p4 = HorizontalBasePoints[4];
        bool randX4 = randomizer.NextBool();
        int x4 = randX4 ? p4.X + randomizer.Next(-10, 30) : p4.X;
        bool randY4 = randomizer.NextBool();
        int y4 = randY4 ? p4.Y + randomizer.Next(10, 50) : p4.Y;
        points.Add(new IntPoint(x4, y4));

        IntPoint p5 = HorizontalBasePoints[5];
        points.Add(new IntPoint(p5.X, p5.Y));

        bool doFlip = randomizer.NextBool();
        if (doFlip)
        {
            points = points.VerticalFlip();
        } 

        return points;
    }

    public IntPointList ReverseOrder()
    {
        IntPointList points = [];
        for(int i = 0; i < this.Count; ++ i)
        {
            points.Add(this[this.Count - 1 - i]);
        }

        return points;
    }

    public IntPointList Swap()
    {
        IntPointList points = [];
        foreach (var point in this)
        {
            points.Add(new IntPoint(point.Y, point.X));
        }

        return points;
    }

    public IntPointList VerticalOffset(int offset)
    {
        IntPointList points = [];
        foreach (var point in this)
        {
            points.Add(new IntPoint(point.X, point.Y + offset));
        }

        return points;
    }

    public IntPointList VerticalFlip()
    {
        IntPointList points = [];
        foreach (var point in this)
        {
            points.Add(new IntPoint(point.X, - point.Y ));
        }

        return points;
    }

    public IntPointList HorizontalOffset(int offset)
    {
        IntPointList points = [];
        foreach (var point in this)
        {
            points.Add(new IntPoint(point.X + offset, point.Y));
        }

        return points;
    }

    public IntPointList HorizontalFlip()
    {
        IntPointList points = [];
        foreach (var point in this)
        {
            points.Add(new IntPoint(-point.X, point.Y));
        }

        return points;
    }
}

