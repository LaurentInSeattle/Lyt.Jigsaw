namespace Lyt.Jigsaw.Model;

using static Lyt.Persistence.FileManagerModel;

public sealed partial class JigsawModel : ModelBase
{
    public bool SetPuzzle(Puzzle puzzle)
    {
        // TODO: Validate puzzle object 
        this.Puzzle = puzzle;
        return true;
    }

    public bool SetPuzzleBackground(double value)
    {
        if (this.Puzzle is null)
        {
            return false;
        }

        this.Puzzle.Background = value;
        new PuzzleChangedMessage(PuzzleChange.Background, value).Publish();
        return true;
    }

    public bool IsPuzzleComplete()
    {
        if (this.Puzzle is null)
        {
            return false;
        }

        return this.Puzzle.IsComplete;
    }

    public bool CheckForPuzzleSnaps(Piece piece)
    {
        if (this.Puzzle is null)
        {
            return false;
        }

        return this.Puzzle.CheckForSnaps(piece);
    }

    public bool RotatePuzzlePiece(Piece piece, bool isCCW)
    {
        if (this.Puzzle is null)
        {
            return false;
        }

        if (piece.IsGrouped)
        {
            piece.Group.Rotate(piece, isCCW);
        }
        else
        {
            piece.Rotate(isCCW);
        }

        return true;
    }

    public bool MovePuzzlePieceTo(Piece piece, double x, double y)
    {
        if (this.Puzzle is null)
        {
            return false;
        }

        piece.MoveTo(x, y);
        return true;
    }

    public List<Piece> GetPuzzleMoves()
    {
        if (this.Puzzle is null)
        {
            return [];
        }

        return this.Puzzle.GetMoves();
    }

    public bool SavePuzzle()
    {
        if (this.Puzzle is null)
        {
            return false;
        }

        try
        {
            // Serialize and save to disk 
            var fileId = new FileId(Area.Desktop, Kind.Json, "Puzzle");
            this.fileManager.Save(fileId, this.Puzzle);

            Debug.WriteLine("Saved");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return false;
        }
    }

    public bool LoadPuzzle()
    {
        //if (this.Puzzle is not null)
        //{
        //    return false;
        //}

        try
        {
            // load from disk and deserialize 
            var fileId = new FileId(Area.Desktop, Kind.Json, "Puzzle");
            Puzzle puzzle = this.fileManager.Load<Puzzle>(fileId);
            puzzle.FinalizeAfterDeserialization();
            this.Puzzle = puzzle;
            Debug.WriteLine("Loaded");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Load, Exception thrown: " + ex);
            return false;
        }
    }
}
