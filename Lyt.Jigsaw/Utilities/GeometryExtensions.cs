namespace Lyt.Jigsaw.Utilities;

using Location = Model.Infrastucture.Location;

public static class GeometryExtensions
{
    public static Point ToPoint(this Location location)
        => new(location.X, location.Y);
}

