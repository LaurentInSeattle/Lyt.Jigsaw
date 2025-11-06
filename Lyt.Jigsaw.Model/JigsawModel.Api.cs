namespace Lyt.Jigsaw.Model;

using static Lyt.Persistence.FileManagerModel;

public sealed partial class JigsawModel : ModelBase
{
    public bool IsPuzzleDirty { get; private set; }

    public bool SetPuzzle(Puzzle puzzle)
    {
        // TODO: Validate puzzle object 
        this.Puzzle = puzzle;

        this.timeoutTimer.Start();
        return true;
    }

    public void PuzzleIsActive() => this.timeoutTimer.ResetTimeout();

    public bool IsPuzzleComplete()
    {
        if (this.Puzzle is null)
        {
            return false;
        }

        bool isComplete = this.Puzzle.IsComplete;
        if (isComplete)
        {
            this.SavePuzzle();
            this.timeoutTimer.Stop();
        }

        return isComplete;
    }

    public bool SetPuzzleBackground(double value) =>
        this.ActionPuzzle(puzzle =>
        {
            puzzle.Background = value;
            new PuzzleChangedMessage(PuzzleChange.Background, value).Publish();
            return true;
        });

    public bool CheckForPuzzleSnaps(Piece piece) => 
        this.ActionPuzzle(puzzle =>
        {
            bool hasMoves = puzzle.CheckForSnaps(piece);
            this.IsPuzzleDirty = hasMoves || this.IsPuzzleDirty;
            return hasMoves;
        });

    public bool RotatePuzzlePiece(Piece piece, bool isCCW) =>
        this.ActionPuzzle(puzzle =>
        {
            if (piece.IsGrouped)
            {
                piece.Group.Rotate(piece, isCCW);
            }
            else
            {
                piece.Rotate(isCCW);
            }
            return true;
        });

    public bool MovePuzzlePieceTo(Piece piece, double x, double y) =>
        this.ActionPuzzle(puzzle =>
        {
            piece.MoveTo(x, y);
            return true;
        });

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
            lock (this.Puzzle)
            {
                // Serialize and save to disk 
                var fileId = new FileId(Area.Desktop, Kind.Json, "Puzzle");
                this.fileManager.Save(fileId, this.Puzzle);
                this.IsPuzzleDirty = false;
            }

            Debug.WriteLine("Puzzle Saved");
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

    private void OnSavePuzzle()
    {
        if (this.Puzzle is null)
        {
            return;
        }

        if (!this.IsPuzzleDirty)
        {
            return;
        }

        this.SavePuzzle();
    }

    private bool ActionPuzzle(Func<Puzzle, bool> action)
    {
        if (this.Puzzle is null)
        {
            return false;
        }

        this.IsPuzzleDirty = true;
        this.timeoutTimer.ResetTimeout();
        lock (this.Puzzle)
        {
            return action(this.Puzzle);
        }
    }
}
