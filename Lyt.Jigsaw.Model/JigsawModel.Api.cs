namespace Lyt.Jigsaw.Model;

using static Lyt.Persistence.FileManagerModel;

public sealed partial class JigsawModel : ModelBase
{
    public bool IsPuzzleDirty { get; private set; }

    public Game? NewGame(
        byte[] imageBytes, byte[] thumbnailBytes,
        int imagePixelHeight, int imagePixelWidth,
        int pieceCount, int rotationSteps, int snap)
    {
        try
        {
            var puzzle = new Puzzle(this.Logger, imagePixelHeight, imagePixelWidth);
            puzzle.Setup(pieceCount, rotationSteps, snap);
            var game = new Game(puzzle);
            this.SetGameAndPuzzle(game, puzzle);
            this.SavePuzzle();
            this.SaveImages(imageBytes, thumbnailBytes);
            this.SaveGame();
            return game;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return null;
        }
    }

    public bool LoadGame()
    {
        return true;
    }


    public bool SetGameAndPuzzle(Game game, Puzzle puzzle)
    {
        // TODO: Validate objects, needed ? 
        this.Game = game;
        this.Puzzle = puzzle;

        this.timeoutTimer.Start();
        return true;
    }

    public void PuzzleIsActive() => this.timeoutTimer.ResetTimeout();

    public bool IsPuzzleComplete()
    {
        if ((this.Game is null) || (this.Puzzle is null))
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
        if ((this.Game is null) || (this.Puzzle is null))
        {
            return [];
        }

        return this.Puzzle.GetMoves();
    }

    public bool SaveGame()
    {
        if ((this.Game is null) || (this.Puzzle is null))
        {
            return false;
        }

        try
        {
            lock (this.Game)
            {
                // Serialize and save to disk
                var fileId = new FileId(Area.User, Kind.Json, this.Game.GameName);
                this.fileManager.Save(fileId, this.Game);
            }

            Debug.WriteLine("Game Saved");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return false;
        }
    }

    public bool SavePuzzle()
    {
        if ((this.Game is null) || (this.Puzzle is null))
        {
            return false;
        }

        try
        {
            lock (this.Puzzle)
            {
                // Serialize and save to disk, puzzle is NOT dirty 
                var fileId = new FileId(Area.User, Kind.Json, this.Game.PuzzleName);
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

    private bool SaveImages(byte[] imageBytes, byte[] thumbnailBytes)
    {
        if ((this.Game is null) || (this.Puzzle is null))
        {
            return false;
        }

        try
        {
            // Save to disk 
            var fileIdImage = new FileId(Area.User, Kind.Binary, this.Game.ImageName);
            this.fileManager.Save(fileIdImage, imageBytes);
            var fileIdThumbnail = new FileId(Area.User, Kind.Binary, this.Game.ThumbnailName);
            this.fileManager.Save(fileIdThumbnail, thumbnailBytes);

            Debug.WriteLine("Images Saved");
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
        if ((this.Game is null) || (this.Puzzle is null))
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
        if ((this.Game is null) || (this.Puzzle is null))
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
