namespace Lyt.Jigsaw.Model.GameObjects;

public sealed class Game
{
#pragma warning disable CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor.
    public Game() {  /* for serialization */ }
#pragma warning restore CS8618 

    public Game(Puzzle puzzle, string imagePath, bool isFilePath)
    {
        this.Puzzle = puzzle;
        this.ImagePath = imagePath;
        this.IsFilePath = isFilePath;
        this.IsCompleted = false;
        this.Started = DateTime.Now;
        this.LastPlayed = DateTime.Now;
        this.Played = TimeSpan.Zero;
    }

    #region Serialized Properties ( Must all be public for both get and set ) 

    public Puzzle Puzzle { get; set; }
    
    public string ImagePath { get; set; }

    public bool IsFilePath { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime Started { get; set; }

    public DateTime LastPlayed { get; set; }

    public TimeSpan Played { get; set; }

    #endregion Serialized Properties ( Must all be public for both get and set ) 

}
