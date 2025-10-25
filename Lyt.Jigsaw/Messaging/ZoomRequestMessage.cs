namespace Lyt.Jigsaw.Messaging;

public sealed record class ZoomRequestMessage(double ZoomFactor, object? Tag = null);
