namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Piece
{
    private readonly Puzzle puzzle; 

    public Piece(Puzzle puzzle, int row, int col)
    {
        this.puzzle = puzzle;
        this.Position = new IntPosition(row, col)   ;
        this.Id = row * puzzle.Columns  + col ;
        this.RotationAngle = puzzle.Randomizer.Next(puzzle.RotationSteps) * puzzle.RotationStepAngle; 
    }

    public int Id { get; private set; }

    public Location Location { get; set; }

    public IntPosition Position { get; private set; }

    public int RotationAngle { get; set; }

    public int TopId { get; private set; }

    public int BottomId { get; private set; }
    
    public int LeftId { get; private set; }
    
    public int RightId { get; private set; }

    public Group? MaybeGroup { get; set; }

    public Group Group 
        => this.MaybeGroup is not null ? 
            this.MaybeGroup : 
            throw new Exception("Should have checked 'IsGrouped'.");

    public bool IsGrouped => this.MaybeGroup is not null;

    public bool IsRotated => this.RotationAngle != 0;

    public bool IsTop => this.Position.Row == 0;

    public bool IsBottom => this.Position.Row == this.puzzle.Rows - 1;
    
    public bool IsLeft => this.Position.Column == 0;
    
    public bool IsRight => this.Position.Column == this.puzzle.Columns - 1;
}
