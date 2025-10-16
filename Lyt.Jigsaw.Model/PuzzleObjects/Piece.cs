namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Piece
{
    public readonly Puzzle Puzzle; 

    public Piece(Puzzle puzzle, int row, int col)
    {
        this.Puzzle = puzzle;
        this.Position = new IntPosition(row, col);
        this.Id = this.Position.ToId(puzzle);
        if ( ! this.IsTop )
        {
            this.TopId = puzzle.ToId(row-1, col);
        }

        if (!this.IsBottom)
        {
            this.BottomId = puzzle.ToId(row + 1, col); 
        }

        if (!this.IsLeft)
        {
            this.LeftId = puzzle.ToId(row, col - 1);
        }

        if (!this.IsRight)
        {
            this.RightId = puzzle.ToId(row, col + 1);
        }

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

    public IntPointList TopPoints { get; private set; } = [];

    public IntPointList BottomPoints { get; private set; } = [];

    public IntPointList LeftPoints { get; private set; } = [];

    public IntPointList RightPoints { get; private set; } = [];

    public SideKind TopSide { get; private set; }

    public SideKind BottomSide { get; private set; }

    public SideKind LeftSide { get; private set; }
    
    public SideKind RightSide { get; private set; }

    public Group? MaybeGroup { get; set; }

    public Group Group 
        => this.MaybeGroup is not null ? 
            this.MaybeGroup : 
            throw new Exception("Should have checked 'IsGrouped'.");

    public bool IsGrouped => this.MaybeGroup is not null;

    public bool IsRotated => this.RotationAngle != 0;

    public bool IsTop => this.Position.Row == 0;

    public bool IsBottom => this.Position.Row == this.Puzzle.Rows - 1;
    
    public bool IsLeft => this.Position.Column == 0;
    
    public bool IsRight => this.Position.Column == this.Puzzle.Columns - 1;

    public Piece GetTop ( )
    {
        if ( this.IsTop )
        {
            throw new Exception("Cant get Top neighbour of Top piece"); 
        }

        if (this.Puzzle.PieceDictionary.TryGetValue( this.TopId, out var top ) && (top is not null))
        {
            return top;
        }

        throw new Exception("Failed to get Top neighbour of non-Top piece");
    }

    public Piece GetBottom()
    {
        if (this.IsBottom)
        {
            throw new Exception("Cant get Bottom neighbour of Bottom piece");
        }

        if (this.Puzzle.PieceDictionary.TryGetValue(this.BottomId, out var bottom) && (bottom is not null))
        {
            return bottom;
        }

        throw new Exception("Failed to get Bottom neighbour of non-Bottom piece");
    }

    public Piece GetLeft()
    {
        if (this.IsLeft)
        {
            throw new Exception("Cant get Left neighbour of Left piece");
        }

        if (this.Puzzle.PieceDictionary.TryGetValue(this.LeftId, out var left) && (left is not null))
        {
            return left;
        }

        throw new Exception("Failed to get Left neighbour of non-Left piece");
    }

    public Piece GetRight()
    {
        if (this.IsRight)
        {
            throw new Exception("Cant get Right neighbour of Right piece");
        }

        if (this.Puzzle.PieceDictionary.TryGetValue(this.RightId, out var right) && (right is not null))
        {
            return right;
        }

        throw new Exception("Failed to get Right neighbour of non-Right piece");
    }

    internal void UpdateSides()
    {
        if ( this.IsTop)
        {
            this.TopSide = SideKind.Flat;
            this.TopPoints = IntPointList.FlatPoints.HorizontalOffset(200).VerticalOffset(200);
        }
        else
        {
            this.TopSide = SideKind.Curved;
            var top = this.GetTop();
            var points = top.BottomPoints.ReverseOrder();
            this.TopPoints = points.VerticalOffset(-1000).VerticalFlip().VerticalOffset(200); 
        }

        if (this.IsLeft)
        {
            this.LeftSide = SideKind.Flat;
            this.LeftPoints = IntPointList.FlatPoints.Swap().HorizontalOffset(200).VerticalOffset(200).ReverseOrder();
        }
        else
        {
            this.LeftSide = SideKind.Curved;
            var left = this.GetLeft();
            var points = left.RightPoints.ReverseOrder();
            this.LeftPoints = points.HorizontalOffset(-1000).HorizontalFlip().HorizontalOffset(200); 
        }

        if ( this.IsBottom)
        {
            this.BottomSide = SideKind.Flat ;
            this.BottomPoints = IntPointList.FlatPoints.HorizontalOffset(200).VerticalOffset(1000).ReverseOrder();
        }
        else
        {
            this.BottomSide = SideKind.Curved;
            this.BottomPoints = IntPointList.RandomizeBasePoints().HorizontalOffset(200).VerticalOffset(1000).ReverseOrder();
        }

        if (this.IsRight)
        {
            this.RightSide = SideKind.Flat;
            this.RightPoints = IntPointList.FlatPoints.Swap().VerticalOffset(200).HorizontalOffset(1000);
        }
        else
        {
            this.RightSide = SideKind.Curved;
            this.RightPoints = IntPointList.RandomizeBasePoints().Swap().VerticalOffset(200).HorizontalOffset(1000);
        }
    }

    internal bool AnySideUnknown =>
        this.TopPoints.Count == 0 ||
        this.BottomPoints.Count == 0 ||
        this.LeftPoints.Count == 0 ||
        this.RightPoints.Count == 0 ||
        this.TopSide == SideKind.Unknown ||
        this.BottomSide == SideKind.Unknown ||
        this.LeftSide == SideKind.Unknown ||
        this.RightSide == SideKind.Unknown;
}
