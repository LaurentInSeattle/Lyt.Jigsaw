namespace Lyt.Jigsaw.Model.Messaging;

public enum PuzzleChange
{
    None = 0,
    Background,
    Progress,
    Hint,
    Start,
    Complete,
}

public sealed record class PuzzleChangedMessage(PuzzleChange Change, double Parameter = 0.0); 