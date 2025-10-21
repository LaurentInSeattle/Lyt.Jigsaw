namespace Lyt.Jigsaw.Model.Infrastucture;

public enum Placement
{
    Unknown,
    Top,
    Bottom,
    Left,
    Right,
}

public static class PlacementExtensions
{
    public static Placement Opposite(this Placement placement)
        => placement switch
        {
            Placement.Top => Placement.Bottom,
            Placement.Bottom => Placement.Top,
            Placement.Left => Placement.Right,
            Placement.Right => Placement.Left,
            _ => throw new Exception("Unknown has no opposite placement "),
        };
}