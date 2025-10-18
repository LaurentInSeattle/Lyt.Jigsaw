namespace Lyt.Jigsaw.Workflow.Game; 

public sealed partial class PieceView : View
{
    private DragMovable? dragMovable;

    public void AttachBehavior(Canvas canvas)
    {
        this.dragMovable = new DragMovable(canvas);
        this.dragMovable.Attach(this);
    }

    ~PieceView()
    {
        this.dragMovable?.Detach();
    }
}
