namespace Lyt.Jigsaw.Workflow.Game;

public class GeometryGenerator
{
    public static PathGeometry CatmullRom(IList<Point> points, bool isClosed)
    {
        var geometry = new PathGeometry();
        using (var context = geometry.Open())
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException("Points"); 
            }

            context.BeginFigure(points[0], false);

            for (int i = 0; i < points.Count - 1; i++)
            {
                Point p0 = (i == 0) ? points[0] : points[i - 1];
                Point p1 = points[i];
                Point p2 = points[i + 1];
                Point p3 = (i == points.Count - 2) ? points[i + 1] : points[i + 2];

                // Tangents at p1 and p2
                Point t1 = new ((p2.X - p0.X) / 2, (p2.Y - p0.Y) / 2);
                Point t2 = new ((p3.X - p1.X) / 2, (p3.Y - p1.Y) / 2);

                // Convert Catmull-Rom to Cubic Bezier control points
                Point control1 = new (p1.X + t1.X / 3, p1.Y + t1.Y / 3);
                Point control2 = new (p2.X - t2.X / 3, p2.Y - t2.Y / 3);

                context.CubicBezierTo(control1, control2, p2);
            }

            if (isClosed)
            {
                context.EndFigure(true);
            }
        }

        return geometry;
    }

    public static PathGeometry BezierControlPoints(IList<Point> points, bool isClosed)
    {
        var geometry = new PathGeometry();
        using (var context = geometry.Open())
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException("Points");
            }

            context.BeginFigure(points[0], false);

            for (int i = 0; i < points.Count - 1; i++)
            {
                Point p0 = (i == 0) ? points[0] : points[i - 1];
                Point p1 = points[i];
                Point p2 = points[i + 1];
                Point p3 = (i == points.Count - 2) ? points[i + 1] : points[i + 2];

                // Tangents at p1 and p2
                Point t1 = new((p2.X - p0.X) / 2, (p2.Y - p0.Y) / 2);
                Point t2 = new((p3.X - p1.X) / 2, (p3.Y - p1.Y) / 2);

                // Convert Catmull-Rom to Cubic Bezier control points
                Point control1 = new(p1.X + t1.X / 3, p1.Y + t1.Y / 3);
                Point control2 = new(p2.X - t2.X / 3, p2.Y - t2.Y / 3);

                context.LineTo(control1);
                context.LineTo(control2);
                context.LineTo(p2);
            }

            if (isClosed)
            {
                context.EndFigure(true);
            }
        }

        return geometry;
    }

    public static PathGeometry Segments(IList<Point> points, bool isClosed)
    {
        var geometry = new PathGeometry();
        using (var context = geometry.Open())
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException("Points");
            }

            context.BeginFigure(points[0], false);

            for (int i = 1; i < points.Count - 1; ++i)
            {
                context.LineTo(points[i]);
            }

            if (isClosed)
            {
                context.EndFigure(true);
            }
        }

        return geometry;
    }
}