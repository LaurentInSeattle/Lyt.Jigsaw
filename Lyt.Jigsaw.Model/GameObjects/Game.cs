namespace Lyt.Jigsaw.Model.GameObjects;

public sealed class Game
{
#pragma warning disable CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor.
    public Game() {  /* for serialization */ }
#pragma warning restore CS8618 

    public Game(Puzzle puzzle)
    {
        this.Name = FileManagerModel.TimestampString(); 
        this.IsCompleted = false;
        this.Started = DateTime.Now;
        this.LastPlayed = DateTime.Now;
        this.Played = TimeSpan.Zero;
        this.Puzzle = puzzle;
    }

    #region Serialized Properties ( Must all be public for both get and set ) 

    public string Name { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime Started { get; set; }

    public DateTime LastPlayed { get; set; }

    public TimeSpan Played { get; set; }

    #endregion Serialized Properties ( Must all be public for both get and set ) 

    [JsonIgnore]
    public Puzzle Puzzle { get; set; }

    public string GameName => string.Concat("Game_", this.Name);

    public string PuzzleName => string.Concat("Puzzle_" , this.Name); 

    public string ImageName => string.Concat("Image_", this.Name);

    public string ThumbnailName => string.Concat("Thumbnail_", this.Name);
}
