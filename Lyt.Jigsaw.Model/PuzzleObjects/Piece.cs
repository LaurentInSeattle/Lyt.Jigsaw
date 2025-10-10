namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Piece
{
    public int Id { get; set; }

    public int Row { get; set; }

    public int Column { get; set; }

    public int Height { get; set; }

    public int Width { get; set; }

    public int RotationAngle { get; set; }

    public bool IsRotated => this.RotationAngle != 0; 

    public double X { get; set; }

    public double Y { get; set; }

    public int TopId { get; set; }

    public int BottomId { get; set; }
    
    public int LeftId { get; set; }
    
    public int RightId { get; set; }
}
