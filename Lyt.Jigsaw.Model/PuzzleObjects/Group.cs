namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Group
{
    private readonly Puzzle puzzle;

    /// <summary> Two pieces joining to create a new group </summary>
    public Group(Puzzle puzzle, Piece first, Piece last)
    {
        if (first.Id == last.Id)
        {
            throw new ArgumentException("Cannot add twice the same piece");
        }

        this.puzzle = puzzle;
        this.Id = first.Id;
        this.AddPiece(first);
        this.AddPiece(last);
    }

    public int Id { get; private set; }

    public List<Piece> Pieces { get; set; } = [];

    public Dictionary<int, Piece> PieceDictionary { get; private set; } = [];

    public Location Location { get; set; }

    public bool HasPiece(Piece piece) => this.PieceDictionary.ContainsKey(piece.Id);

    /// <summary> Single piece merging into this group </summary>
    public bool CanAddPiece(Piece piece)
    {
        // TODO !
        if (piece.IsGrouped)
        {
            throw new ArgumentException("Piece already belongs to another group");
        }

        if (this.HasPiece(piece))
        {
            throw new ArgumentException("Cannot add twice the same piece");
        }

        return true;
    }

    public bool AddPiece(Piece piece)
    {
        if (!this.CanAddPiece(piece))
        {
            return false;
        } 

        piece.Group = this;
        this.Pieces.Add(piece);
        this.PieceDictionary.Add(piece.Id, piece);
        return true;
    }

    public bool CanAddGroup(Group group)
    {
        if ( ( this == group) ||(this.Id == group.Id))
        {
            throw new ArgumentException("Cannot merge the same groups");
        }

        // TODO: Make sure that no piece belongs to both groups 
        return true;
    }

    /// <summary> Other group merging into this one </summary>
    public bool AddGroup(Group group)
    {
        if (!this.CanAddGroup(group))
        {
            return false;
        }

        foreach (var piece in group.Pieces)
        {
            piece.UnGroup(); 
            bool success = this.AddPiece(piece);
            if (!success)
            {
                throw new Exception("Failed to add piece, Id: " + piece.Id);
            }
        }

        group.Pieces.Clear();
        group.PieceDictionary.Clear();
        return true;
    }

    internal void MoveBy(Piece piece, double deltaX, double deltaY)
    {
        foreach (Piece other in this.Pieces)
        {
            if (piece == other)
            {
                continue; 
            }

            other.MoveBy(deltaX, deltaY, save: false); 
        }
    }

    public void Rotate(Piece piece, bool isCCW)
    {
        this.puzzle.Moves.Clear();
        
        double angle = this.puzzle.RotationStepAngle;
        if (!isCCW)
        {
            angle = -angle;
        } 

        angle = Math.Tau * angle / 360.0;
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);
        double halfSize = puzzle.PieceSize / 2.0;

        // Normalized clicked piece center coordinates 
        double pCx = piece.Center.X;
        double pCy = -piece.Center.Y;

        foreach (Piece other in this.Pieces)
        {
            // All pieces rotate, the clicked piece does not move, just rotate 
            // But all are treated as moves 
            this.puzzle.Moves.Add(other); 
            other.Rotate(isCCW, false);             
            if (piece == other)
            {
                // clicked piece does not move, done 
                continue;
            }

            // Move all others by rotating their centers around the center of the clicked piece

            // Normalize center coordinates 
            double oCx = other.Center.X;
            double oCy = - other.Center.Y;

            // Recenter 
            double deltaX = oCx - pCx;
            double deltaY = oCy - pCy;

            // rotate 
            double x = deltaX * cos - deltaY * sin;
            double y = deltaX * sin + deltaY * cos;

            // Denormalize 
            y = -y; 

            // Recenter 
            x += piece.Center.X;
            y += piece.Center.Y;

            // x y == new center, adjust for top left position on canvas 
            x -= halfSize;
            y -= halfSize;

            // Update
            other.Location = new(x, y);
        }
    }
}
