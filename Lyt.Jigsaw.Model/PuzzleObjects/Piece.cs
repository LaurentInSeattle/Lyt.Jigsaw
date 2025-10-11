namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Piece
{
    public int Id { get; set; }

    public Location Location { get; set; }

    public IntPosition Position { get; set; }

    public int RotationAngle { get; set; }

    public bool IsRotated => this.RotationAngle != 0; 

    public int TopId { get; set; }

    public int BottomId { get; set; }
    
    public int LeftId { get; set; }
    
    public int RightId { get; set; }
}
