namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Piece
{
#pragma warning disable CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor.
    public Piece()  { /* For serialization */ }
#pragma warning restore CS8618 

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
        this.Rotate();
        this.IsVisible = true;
    }

    #region Serialized Properties ( Must all be public for both get and set ) 

    // Can be zero
    public int Id { get; set; }

    // CANNOT be zero
    public int GroupId { get; set; }

    // Can be zero
    public int SnapPieceId { get; set; }

    public int TopId { get; set; }

    public int BottomId { get; set; }

    public int LeftId { get; set; }

    public int RightId { get; set; }

    public Location Location { get; set; }

    public IntPosition Position { get; set; }

    public int RotationSteps { get; set; }

    public int RotationAngle { get; set; }

    public IntPointList TopPoints { get; set; } = [];

    public IntPointList BottomPoints { get; set; } = [];

    public IntPointList LeftPoints { get; set; } = [];

    public IntPointList RightPoints { get; set; } = [];

    #endregion // Serialized Properties 

    // Not serialized 
    public Puzzle Puzzle { get; private set; }

    public Location Center =>
        new(this.Location.X + this.Puzzle.PieceSize / 2, this.Location.Y + this.Puzzle.PieceSize / 2);

    public Location CenterTop =>
        new(this.Location.X + this.Puzzle.PieceSize / 2, this.Location.Y );

    public Location CenterBottom =>
        new(this.Location.X + this.Puzzle.PieceSize / 2, this.Location.Y + this.Puzzle.PieceSize);

    public Location CenterLeft =>
        new(this.Location.X , this.Location.Y + this.Puzzle.PieceSize / 2);

    public Location CenterRight =>
        new(this.Location.X + this.Puzzle.PieceSize, this.Location.Y + this.Puzzle.PieceSize / 2);

    [JsonIgnore]
    public bool IsVisible { get; private set; }

    [JsonIgnore]
    internal Group? MaybeGroup { get; private set; }

    [JsonIgnore]
    public Group Group
    {
        get =>
            this.MaybeGroup is not null ?
                this.MaybeGroup :
                throw new Exception("Should have checked 'IsGrouped'.");
        set => this.MaybeGroup = value;
    }

    public bool IsGrouped => this.GroupId > 0 && this.MaybeGroup is not null;

    public bool IsEdge => this.IsTop || this.IsLeft || this.IsRight || this.IsBottom;

    internal bool IsTop => this.Position.Row == 0;

    internal bool IsBottom => this.Position.Row == this.Puzzle.Rows - 1;

    internal bool IsLeft => this.Position.Column == 0;

    internal bool IsRight => this.Position.Column == this.Puzzle.Columns - 1;

    internal void UnGroup()
    {
        this.GroupId = -1;
        this.MaybeGroup = null;
    }

    internal Piece GetTop()
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

    internal Piece GetBottom()
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

    internal Piece GetLeft()
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

    internal Piece GetRight()
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
            this.TopPoints = IntPointList.FlatPoints.Offset(200, 200);
        }
        else
        {
            var top = this.GetTop();
            var points = top.BottomPoints.ReverseOrder();
            this.TopPoints = points.Offset(0, -800);
        }

        if (this.IsLeft)
        {
            this.LeftPoints = IntPointList.FlatPoints.Swap().Offset(200, 200).ReverseOrder();
        }
        else
        {
            var left = this.GetLeft();
            var points = left.RightPoints.ReverseOrder();
            this.LeftPoints = points.Offset(-800, 0);
        }

        this.BottomPoints =
            this.IsBottom ?
                IntPointList.FlatPoints.Offset(200, 1000).ReverseOrder() :
                IntPointList.RandomizeBasePoints().Offset(200, 1000).ReverseOrder();
        this.RightPoints =
            this.IsRight ?
                IntPointList.FlatPoints.Swap().Offset(1000, 200) :
                IntPointList.RandomizeBasePoints().Swap().Offset(1000, 200);
    }

    public void Rotate(bool isCCW = true)
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
    }

    public void MoveTo(double x, double y)
    {
        double deltaX = x - this.Location.X;
        double deltaY = y - this.Location.Y;
        this.Location = new Location(x, y);

        if (this.IsGrouped)
        {
            // Move the rest of the group by 
            this.Group.MoveBy(this, deltaX, deltaY);
        }
    }

    public void MoveBy(double deltaX, double deltaY)
        => this.Location = new Location(this.Location.X + deltaX, this.Location.Y + deltaY);

    internal bool IsSharingOneSideWith(Piece targetPiece, out Placement placement)
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

    internal void SnapTargetToThis(Piece targetPiece, Placement placement)
    {
        //if (placement == Placement.Unknown)
        //{
        //    if (Debugger.IsAttached) { Debugger.Break(); }
        //    return;
        //}

        this.Puzzle.Moves.Add(targetPiece);
        var location = this.SnapLocation(placement);
        targetPiece.MoveTo(location.X, location.Y);
    }

    internal void ManageGroups(Piece targetPiece)
    {
        var groups = this.Puzzle.Groups;
        if (this.IsGrouped && targetPiece.IsGrouped)
        {
            // two groups are merging into one 
            var groupToRemove1 = this.Group;
            var groupToRemove2 = targetPiece.Group;
            groups.Add(new Group(this.Group, targetPiece.Group));

            // Do not use : targetPiece.Group ot this.Group
            // because target and this have changed of group, and this
            // would delete the newly created one
            groups.Remove(groupToRemove1);
            groups.Remove(groupToRemove2);

            // Debug.WriteLine("Groups merging");
        }
        else if (this.IsGrouped && !targetPiece.IsGrouped)
        {
            // target piece is joining the group this piece belongs to
            this.Group.AddPiece(targetPiece);
            
            // Debug.WriteLine("Group growing");
        }
        else if (!this.IsGrouped && targetPiece.IsGrouped)
        {
            // group of target piece is joining with this piece
            // similar as above reversing roles of this piece and target piece 
            targetPiece.Group.AddPiece(this);
            
            // Debug.WriteLine("Group growing");
        }
        else
        {
            // two non-grouped pieces creating the first group 
            groups.Add(new Group(this, targetPiece));
            
            // Debug.WriteLine("New Group");
        }

        // Debug.WriteLine("Groups: " + groups.Count);
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

    internal void FinalizeAfterDeserialization(Puzzle puzzle)
    {
        this.Puzzle = puzzle;
        this.IsVisible = true;
        if (this.GroupId > 0)
        {
            var group = this.Puzzle.Groups.Where(group => group.Id == this.GroupId).FirstOrDefault();
            if (group is not null)
            {
                this.Group = group;
            }
        }
    }

    internal bool AnySideUnknown =>
        this.TopPoints.Count == 0 ||
        this.BottomPoints.Count == 0 ||
        this.LeftPoints.Count == 0 ||
        this.RightPoints.Count == 0;
}
