namespace Lyt.Jigsaw.Model.GameObjects;

/// <summary> Serialised game statistics </summary>
public sealed class GameStatistics
{
    public int TotalGamesStarted { get; set; }

    public int TotalGamesCompleted { get; set; }

    public TimeSpan TotalTimePlayed { get; set; }

    public int TotalHintsUsed { get; set; }

    public int MaxPieceCount { get; set; }

    public int TotalPiecesSnapped { get; set; }

    public int TotalPiecesRotated { get; set; }

    public int LongestGamePieceCount { get; set; }

    public DateTime LongestGameDate { get; set; }

    public TimeSpan LongestGameTimePlayed { get; set; }
}
