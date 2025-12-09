namespace Lyt.Jigsaw.Model.GameObjects;

public sealed class Game
{
#pragma warning disable CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor.
    public Game() {  /* for serialization */ }
#pragma warning restore CS8618 

    public Game(Puzzle puzzle, PuzzleParameters puzzleParameters)
    {
        this.Name = FileManagerModel.TimestampString();
        this.IsCompleted = false;
        this.Started = DateTime.Now;
        this.LastPlayed = DateTime.Now;
        this.Played = TimeSpan.Zero;
        this.Puzzle = puzzle;
        this.PuzzleParameters = puzzleParameters;
    }

    #region Serialized Properties ( Must all be public for both get and set ) 

    public string Name { get; set; }

    public bool IsCompleted { get; set; }

    public int Progress { get; set; }

    public int PieceCount { get; set; }

    public int HintsUsed { get; set; }

    public DateTime Started { get; set; }

    public DateTime LastPlayed { get; set; }

    public TimeSpan Played { get; set; }

    public PuzzleParameters PuzzleParameters { get; set; }

    #endregion Serialized Properties ( Must all be public for both get and set ) 

    [JsonIgnore]
    public Puzzle Puzzle { get; set; }

    public static string GameNameFromKey(string key) => string.Concat("Game_", key);

    public static string PuzzleNameFromKey(string key) => string.Concat("Puzzle_", key);

    public static string ImageNameFromKey(string key) => string.Concat("Image_", key);
    
    public static string ThumbnailNameFromKey(string key) => string.Concat("Thumbnail_", key);

    public string GameName => GameNameFromKey(this.Name);

    public string PuzzleName => PuzzleNameFromKey(this.Name);

    public string ImageName => ImageNameFromKey(this.Name);

    public string ThumbnailName => ThumbnailNameFromKey(this.Name);
}
