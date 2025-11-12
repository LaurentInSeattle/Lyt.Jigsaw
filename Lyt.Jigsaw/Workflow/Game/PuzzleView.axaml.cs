namespace Lyt.Jigsaw.Workflow.Game;

public sealed partial class PuzzleView: View
{
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            new ToolbarCommandMessage(ToolbarCommandMessage.ToolbarCommand.PlayWindowed).Publish();
        } 

        base.OnKeyDown(e);
    }
}
