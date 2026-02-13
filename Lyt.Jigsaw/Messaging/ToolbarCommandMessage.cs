namespace Lyt.Jigsaw.Messaging;

public sealed record class ToolbarCommandMessage(
    ToolbarCommandMessage.ToolbarCommand Command, object? CommandParameter = null)
{
    public enum ToolbarCommand
    {
        // Left - Main toolbar in Shell view 
        Today,
        Collection,
        Settings,
        About,

        // Right - Main toolbar in Shell view  
        Close,

        // Play toolbar
        PlayFullscreen,

        // Collection toolbars 
        Play,
        CollectionSaveToDesktop,
        RemoveFromCollection,   // Collection Only

        // Settings toolbars 
        Cleanup,
        PlayWindowed,
        Rearrange,
    }
}
