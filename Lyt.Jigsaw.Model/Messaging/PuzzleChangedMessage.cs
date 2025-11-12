namespace Lyt.Jigsaw.Model.Messaging;

public enum PuzzleChange
{
    None = 0,
    Background,
    Progress,
}

public sealed record class PuzzleChangedMessage(PuzzleChange Change, double Parameter = 0.0); 