namespace Lyt.Jigsaw.Model.PuzzleObjects;

using System.Collections.Generic;

public sealed class Group
{
#pragma warning disable CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor.
    public Group() { /* For serialization */ }
#pragma warning restore CS8618 

    internal Group(Group group1, Group group2)
    {
        this.Id = group1.Id;
        this.AddGroup(group1);
        this.AddGroup(group2);
    }

    /// <summary> Two pieces joining to create a new group </summary>
    internal Group(Piece first, Piece last)
    {
        if (first.Id == last.Id)
        {
            throw new ArgumentException("Cannot add twice the same piece");
        }

        // Group ID cannot be zero 
        this.Id = 1 + first.Id;
        this.AddPiece(first);
        this.AddPiece(last);
    }

    #region Serialized Properties 

    public int Id { get; set; }

    public HashSet<int> PieceIds { get; set; } = [];

    #endregion Serialized Properties 

    [JsonIgnore]
    public List<Piece> Pieces { get; set; } = [];

    public void Rotate(Piece piece, bool isCCW)
    {
        Puzzle puzzle = piece.Puzzle; 
        puzzle.Moves.Clear();

        double angle = puzzle.RotationStepAngle;
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
            puzzle.Moves.Add(other);
            other.Rotate(isCCW);
            if (piece == other)
            {
                // clicked piece does not move, done 
                continue;
            }

            // Move all others by rotating their centers around the center of the clicked piece

            // Normalize center coordinates 
            double oCx = other.Center.X;
            double oCy = -other.Center.Y;

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

    internal bool AddPiece(Piece piece)
    {
        if (!this.CanAddPiece(piece))
        {
            return false;
        } 

        piece.Group = this;
        piece.GroupId = this.Id; 
        this.PieceIds.Add(piece.Id);
        this.Pieces.Add(piece);
        return true;
    }

    /// <summary> Other group merging into this one </summary>
    internal bool AddGroup(Group group)
    {
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
        group.PieceIds.Clear();
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

            other.MoveBy(deltaX, deltaY); 
        }
    }

    private bool HasPiece(Piece piece) => this.PieceIds.Contains(piece.Id);

    /// <summary> Single piece merging into this group </summary>
    private bool CanAddPiece(Piece piece)
    {
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

    internal void FinalizeAfterDeserialization(Puzzle puzzle)
    {
        // Rebuild the list of pieces
        foreach (int pieceId in this.PieceIds)
        {
            Piece piece = puzzle.PieceDictionary[pieceId];
            this.Pieces.Add(piece);
        }
    }
}
