namespace Lyt.Jigsaw.Model;

using static Lyt.Persistence.FileManagerModel;

public sealed partial class JigsawModel : ModelBase
{
    public bool IsPuzzleDirty { get; private set; }

    public bool IsGameActive { get; private set; }

    public void GameIsActive(bool isActive = true) => this.IsGameActive = isActive;

    public Game? NewGame(
        byte[] imageBytes, byte[] thumbnailBytes,
        int imagePixelHeight, int imagePixelWidth,
        PuzzleImageSetup setup,
        PuzzleParameters puzzleParameters)
    {
        try
        {
            var puzzle = new Puzzle(this.Logger);
            puzzle.Setup(setup, puzzleParameters);
            var game = new Game(puzzle, puzzleParameters);
            this.Game = game;
            this.Puzzle = puzzle;
            this.SavePuzzle();
            this.SaveImages(imageBytes, thumbnailBytes);
            this.SaveGame();
            this.timeoutTimer.Start();
            return game;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return null;
        }
    }

    #region In-Game Puzzle Actions 

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
            this.SaveGame();
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

    public void ProvidePuzzleHint()
    {
        this.ActionPuzzle(puzzle =>
        {
            bool gotHint = puzzle.ProvideHint();
            if (gotHint)
            {
                int progress = puzzle.Progress();
                new PuzzleChangedMessage(PuzzleChange.Hint).Publish();
                new PuzzleChangedMessage(PuzzleChange.Progress, progress).Publish();
            }

            return gotHint;
        });
    }

    public bool CheckForPuzzleSnaps(Piece piece) =>
        this.ActionPuzzle(puzzle =>
        {
            bool hasMoves = puzzle.CheckForSnaps(piece);
            this.IsPuzzleDirty = hasMoves || this.IsPuzzleDirty;
            if (hasMoves)
            {
                this.IsPuzzleComplete();
                new PuzzleChangedMessage(PuzzleChange.Progress, puzzle.Progress()).Publish();
            }

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

    public int GetPuzzleProgress()
    {
        if ((this.Game is null) || (this.Puzzle is null))
        {
            return 0;
        }

        return this.Puzzle.Progress();
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

    #endregion In-Game Puzzle Actions 

    #region Load and Save 

    public byte[]? LoadGame(string gameKey)
    {
        try
        {
            // Load from disk and deserialize
            string gameName = Game.GameNameFromKey(gameKey);
            var fileId = new FileId(Area.User, Kind.Json, gameName);
            var game =
                this.fileManager.Load<Game>(fileId) ??
                throw new Exception("Failed to deserialize");

            this.Game = game;
            this.LoadPuzzle();
            byte[]? imageBytes = this.LoadImage();
            if ((imageBytes is null) || (imageBytes.Length < 256))
            {
                throw new Exception("Failed to read image from disk: " + gameName);
            }

            this.timeoutTimer.Start();

            Debug.WriteLine("Game Loaded");
            return imageBytes;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Load, Exception thrown: " + ex);
            return null;
        }
    }

    // Replicate the puzzle completion state to the game so that we have it 
    // to show in game lists without having to load the entire puzzle
    private void ReplicatePuzzleState()
    {
        if ((this.Game is null) || (this.Puzzle is null))
        {
            return;
        }

        this.Game.IsCompleted = this.Puzzle.IsComplete;
        this.Game.Progress = this.Puzzle.Progress();
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
                this.ReplicatePuzzleState();

                // Serialize and save to disk
                var fileId = new FileId(Area.User, Kind.Json, this.Game.GameName);
                this.fileManager.Save(fileId, this.Game);

                if (this.SavedGames.ContainsKey(this.Game.Name))
                {
                    this.SavedGames[this.Game.Name] = this.Game;
                }
                else
                {
                    this.SavedGames.Add(this.Game.Name, this.Game);
                }
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

    public bool DeleteGame(string key, out string message)
    {
        message = string.Empty;
        if (this.Game is not null && this.IsGameActive)
        {
            if (this.Game.Name == key)
            {
                // Cannot delete the game that is currently loaded
                message = "Cannot delete the game that is currently loaded.";
                return false;
            }
        }

        try
        {
            // Delete from disk the four files 
            var fileId = new FileId(Area.User, Kind.Json, Game.GameNameFromKey(key));
            this.fileManager.Delete(fileId);
            fileId = new FileId(Area.User, Kind.JsonCompressed, Game.PuzzleNameFromKey(key));
            this.fileManager.Delete(fileId);
            fileId = new FileId(Area.User, Kind.Binary, Game.ImageNameFromKey(key));
            this.fileManager.Delete(fileId);
            fileId = new FileId(Area.User, Kind.Binary, Game.ThumbnailNameFromKey(key));
            this.fileManager.Delete(fileId);

            // Clear in memory data 
            this.SavedGames.Remove(key);
            this.ThumbnailCache.Remove(key);

            Debug.WriteLine("Game Deleted");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Delete, Exception thrown: " + ex);
            message = "Delete, Exception thrown: " + ex.Message;
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
                this.ReplicatePuzzleState();

                // Serialize and save to disk, puzzle is NOT dirty 
                var fileId = new FileId(Area.User, Kind.JsonCompressed, this.Game.PuzzleName);
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

    private byte[]? LoadImage()
    {
        if ((this.Game is null) || (this.Puzzle is null))
        {
            return null;
        }

        try
        {
            // Load from disk 
            var fileIdImage = new FileId(Area.User, Kind.Binary, this.Game.ImageName);
            byte[] imageBytes = this.fileManager.Load<byte[]>(fileIdImage);

            Debug.WriteLine("Image Loaded");
            return imageBytes;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return null;
        }
    }

    public byte[]? GetThumbnail(string name)
    {
        if (!this.ThumbnailCache.TryGetValue(name, out byte[]? thumbnailBytes))
        {
            try
            {
                // Load from disk 
                var fileIdImage = new FileId(Area.User, Kind.Binary, Game.ThumbnailNameFromKey(name));
                byte[] imageBytes = this.fileManager.Load<byte[]>(fileIdImage);

                Debug.WriteLine("Thumbnail Loaded");
                this.ThumbnailCache.Add(name, imageBytes);
                return imageBytes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Get Thumbnail, Exception thrown: " + ex);
                return null;
            }
        }

        return thumbnailBytes;
    }

    public bool LoadPuzzle()
    {
        if (this.Game is null)
        {
            return false;
        }

        try
        {
            // load from disk and deserialize 
            var fileId = new FileId(Area.User, Kind.JsonCompressed, this.Game.PuzzleName);
            Puzzle puzzle = this.fileManager.Load<Puzzle>(fileId);
            puzzle.FinalizeAfterDeserialization(this.Logger);
            this.Puzzle = puzzle;

            Debug.WriteLine("Puzzle Loaded");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Puzzle Load, Exception thrown: " + ex);
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

    #endregion Load and Save 

}
