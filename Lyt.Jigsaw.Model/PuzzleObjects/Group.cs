namespace Lyt.Jigsaw.Model.PuzzleObjects;

using System.Collections.Generic;
using System.IO.Pipelines;

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

        return false;
    }

    public bool AddPiece(Piece piece)
    {
        if (!this.CanAddPiece(piece))
        {
            return false;
        } 

        this.Pieces.Add(piece);
        this.PieceDictionary.Add(piece.Id, piece);
        return true;
    }

    public bool CanAddGroup(Group group)
    {
        // TODO !
        if (this.Id == group.Id)
        {
            throw new ArgumentException("Cannot merge the same groups");
        }

        return false;
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
}
