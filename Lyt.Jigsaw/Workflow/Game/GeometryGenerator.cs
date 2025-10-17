namespace Lyt.Jigsaw.Workflow.Game;

public static class GeometryGenerator
{
    public static List<Point> ToPoints(this IntPointList intPoints)
    {
        var points = new List<Point>();
        foreach (var point in intPoints)
        {
            points.Add(new Point(point.X, point.Y));
        }

        return points;
    }

    public static List<Point> ToScaledPoints(this IntPointList intPoints, double scale)
    {
        var points = new List<Point>();
        foreach (var point in intPoints)
        {
            points.Add(new Point(point.X, point.Y) * scale);
        }

        return points;
    }

    // Combine the two geometries with the 'Intersect' mode.
    public static Geometry InvertedClip(Geometry outerGeometry, Geometry innerGeometry)
        => new CombinedGeometry(GeometryCombineMode.Intersect, outerGeometry, innerGeometry);

    public static Geometry Combine(params IEnumerable<IList<Point>> pointsLists)
    {
        if (pointsLists == null || !pointsLists.Any())
        {
            throw new ArgumentException("Points");
        }

        var pathGeometry = new PathGeometry() { Figures = [] };
        var segment = new PolyBezierSegment { Points = [] };
        var pathFigure = new PathFigure()
        {
            IsFilled = true,
            IsClosed = false,
            Segments = [],
        };

        foreach (var points in pointsLists)
        {
            if (points == null || points.Count <= 1)
            {
                throw new ArgumentException("Points");
            }

            // Convert Catmull-Rom to Cubic Bezier control points
            pathFigure.StartPoint = points[0];
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

                // context.CubicBezierTo(control1, control2, p2);
                segment.Points.Add(control1);
                segment.Points.Add(control2);
                segment.Points.Add(p2);
            }
        }

        pathFigure.Segments.Add(segment);
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    #region Unused - Keep for now 

    public static PathGeometry CatmullRom(IList<Point> points, bool isFilled = false, bool isClosed = false)
    {
        if (points == null || points.Count < 2)
        {
            throw new ArgumentException("Points");
        }

        var geometry = new PathGeometry();
        using (var context = geometry.Open())
        {
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

                context.CubicBezierTo(control1, control2, p2);
            }

            context.EndFigure(isClosed);
        }

        return geometry;
    }

    public static PathGeometry BezierControlPoints(IList<Point> points, bool isClosed = false)
    {
        if (points == null || points.Count < 2)
        {
            throw new ArgumentException("Points");
        }

        var geometry = new PathGeometry();
        using (var context = geometry.Open())
        {
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

    public static PathGeometry Segments(IList<Point> points, bool isClosed = false)
    {
        if (points == null || points.Count < 2)
        {
            throw new ArgumentException("Points");
        }

        var geometry = new PathGeometry();
        using (var context = geometry.Open())
        {
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

    public static Geometry Combine(params IEnumerable<PathGeometry> pathGeometries)
    {
        var geometryGroup = new GeometryGroup();
        foreach (var pathGeometry in pathGeometries)
        {
            geometryGroup.Children.Add(pathGeometry);
        }

        return geometryGroup;
    }
    
    #endregion Unused 
}