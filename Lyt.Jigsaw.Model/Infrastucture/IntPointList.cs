namespace Lyt.Jigsaw.Model.Infrastucture;

public sealed class IntPointList : List<IntPoint> , IList<IntPoint>
{
    public static readonly List<IntPoint> HorizontalBasePoints =
    [
        // Base
        new (0, 0),
        new (300, 20),
        new (350, -120),
        new (450, -120),
        new (500, 20),
        new (800, 0),

        // Variant
        //new (0, 0),
        //new (300, 20),
        //new (350, -160),
        //new (450, -100),
        //new (500, 20),
        //new (800, 0),

        // Variant
        //new (0, 0),
        //new (350, 20),
        //new (350, -120),
        //new (450, -120),
        //new (480, 30),
        //new (800, 0),
    ];

    public static IntPointList RandomizeBasePoints()
    {
        IntPointList points = [];
        foreach (var point in HorizontalBasePoints)
        {
            points.Add(new IntPoint(point.X, point.Y));
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

