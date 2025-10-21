namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Piece
{
    public readonly Puzzle Puzzle;

    public Piece(Puzzle puzzle, int row, int col)
    {
        this.Puzzle = puzzle;
        this.Position = new IntPosition(row, col);
        this.Id = this.Position.ToId(puzzle);
        if (!this.IsTop)
        {
            this.TopId = puzzle.ToId(row - 1, col);
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

        this.RotationSteps = puzzle.Randomizer.Next(puzzle.RotationSteps);
        this.Rotate(save: false);
    }

    public int Id { get; private set; }

    public Location Location { get; set; }

    public IntPosition Position { get; private set; }

    public int RotationSteps { get; set; }

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

    public Group? MaybeGroup { get; private set; }

    public Group Group
    {
        get => this.MaybeGroup is not null ?
            this.MaybeGroup :
            throw new Exception("Should have checked 'IsGrouped'.");
        set => this.MaybeGroup = value;
    }

    public bool IsGrouped => this.MaybeGroup is not null;

    public bool IsRotated => this.RotationAngle != 0;

    public bool IsTop => this.Position.Row == 0;

    public bool IsBottom => this.Position.Row == this.Puzzle.Rows - 1;

    public bool IsLeft => this.Position.Column == 0;

    public bool IsRight => this.Position.Column == this.Puzzle.Columns - 1;

    public Placement SnapPlacement { get; private set; } = Placement.Unknown;

    public Piece? MaybeSnapPiece { get; private set; }

    public bool IsSnapped => this.MaybeSnapPiece is not null && this.SnapPlacement != Placement.Unknown;

    public Piece SnapPiece
    {
        get => this.MaybeSnapPiece is not null ?
                this.SnapPiece :
                throw new Exception("Should have checked 'IsSnapped'.");
        set => this.MaybeSnapPiece = value;
    }

    public Piece GetTop()
    {
        if (this.IsTop)
        {
            throw new Exception("Cant get Top neighbour of Top piece");
        }

        if (this.Puzzle.PieceDictionary.TryGetValue(this.TopId, out var top) && (top is not null))
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
        if (this.IsTop)
        {
            this.TopSide = SideKind.Flat;
            this.TopPoints = IntPointList.FlatPoints.Offset(200, 200);
        }
        else
        {
            this.TopSide = SideKind.Curved;
            var top = this.GetTop();
            var points = top.BottomPoints.ReverseOrder();
            this.TopPoints = points.Offset(0, -800);
        }

        if (this.IsLeft)
        {
            this.LeftSide = SideKind.Flat;
            this.LeftPoints = IntPointList.FlatPoints.Swap().Offset(200, 200).ReverseOrder();
        }
        else
        {
            this.LeftSide = SideKind.Curved;
            var left = this.GetLeft();
            var points = left.RightPoints.ReverseOrder();
            this.LeftPoints = points.Offset(-800, 0);
        }

        if (this.IsBottom)
        {
            this.BottomSide = SideKind.Flat;
            this.BottomPoints = IntPointList.FlatPoints.Offset(200, 1000).ReverseOrder();
        }
        else
        {
            this.BottomSide = SideKind.Curved;
            this.BottomPoints = IntPointList.RandomizeBasePoints().Offset(200, 1000).ReverseOrder();
        }

        if (this.IsRight)
        {
            this.RightSide = SideKind.Flat;
            this.RightPoints = IntPointList.FlatPoints.Swap().Offset(1000, 200);
        }
        else
        {
            this.RightSide = SideKind.Curved;
            this.RightPoints = IntPointList.RandomizeBasePoints().Swap().Offset(1000, 200);
        }
    }

    public void Rotate(bool isCCW = true, bool save = true)
    {
        if (isCCW)
        {
            --this.RotationSteps;
            if (this.RotationSteps < 0)
            {
                this.RotationSteps = this.Puzzle.RotationSteps - 1;
            }
        }
        else
        {
            ++this.RotationSteps;
            if (this.RotationSteps >= this.Puzzle.RotationSteps)
            {
                this.RotationSteps = 0;
            }
        }

        this.RotationAngle = this.RotationSteps * this.Puzzle.RotationStepAngle;
        if (save)
        {
            this.Puzzle.Save();
        }
    }

    public void MoveTo(double x, double y, bool save = true)
    {
        double deltaX = x - this.Location.X;
        double deltaY = y - this.Location.Y;
        this.Location = new Location(x, y);

        if (this.IsGrouped)
        {
            // Move the rest of the group by 
            this.Group.MoveBy(this, deltaX, deltaY);
        }

        if (save)
        {
            this.Puzzle.Save();
        }
    }

    public void MoveBy(double deltaX, double deltaY, bool save = true)
    {
        this.Location = new Location(this.Location.X + deltaX, this.Location.Y + deltaY);

        if (save)
        {
            this.Puzzle.Save();
        }
    }

    internal bool ShareOneSideWith(Piece targetPiece, out Placement placement)
    {
        placement = Placement.Unknown;

        if ((!this.IsTop) && (this.GetTop() == targetPiece))
        {
            placement = Placement.Top;
            return true;
        }

        if ((!this.IsBottom) && (this.GetBottom() == targetPiece))
        {
            placement = Placement.Bottom;
            return true;
        }

        if ((!this.IsLeft) && (this.GetLeft() == targetPiece))
        {
            placement = Placement.Left;
            return true;
        }

        if ((!this.IsRight) && (this.GetRight() == targetPiece))
        {
            placement = Placement.Right;
            return true;
        }

        return false;
    }

    internal void SnapTo(Piece targetPiece, Placement placement)
    {
        var location = this.SnapLocation(placement);
        targetPiece.MoveTo(location.X, location.Y);
        targetPiece.SnapPiece = this;
        targetPiece.SnapPlacement = placement;
        if (!this.IsSnapped)
        {
            this.SnapPiece = targetPiece;
            this.SnapPlacement = placement.Opposite();
        }

        if (this.IsGrouped && targetPiece.IsGrouped)
        {
            // two groups are merging  
        }
        else if (this.IsGrouped && !targetPiece.IsGrouped)
        {
            // target piece is joining the group this piece belongs to
            this.Group.AddPiece(targetPiece);
        }
        else if (!this.IsGrouped && targetPiece.IsGrouped)
        {
            // group of target piece is joining with this piece
            // similar as above reversing roles of this piece and target piece 
            targetPiece.Group.AddPiece(this);
        }
        else
        {
            // two non-grouped pieces creating the first group 
            this.Puzzle.Groups.Add(new Group(this.Puzzle, this, targetPiece));
        }
    }

    internal Location SnapLocation(Placement placement)
    {
        double angle = placement switch
        {
            Placement.Top => -90.0,
            Placement.Right => 0.0,
            Placement.Bottom => 90.0,
            Placement.Left => 180.0,
            _ => throw new Exception("Unknown placement"),
        };

        angle += this.RotationAngle;
        angle = -angle;
        angle = Math.Tau * angle / 360.0;
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);
        double radius = this.Puzzle.PieceSize;
        double x = this.Location.X + radius * cos;
        double y = this.Location.Y - radius * sin;
        return new(x, y);
    }

    public Location Center =>
        new(this.Location.X + this.Puzzle.PieceSize / 2, this.Location.Y + this.Puzzle.PieceSize / 2);

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
