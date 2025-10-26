namespace Lyt.Jigsaw.Workflow.Game;

using Location = Model.Infrastucture.Location;

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

    public void MoveTo(Location location)
    {
        this.SetValue(Canvas.LeftProperty, location.X);
        this.SetValue(Canvas.TopProperty, location.Y);
    }

    public void MovePieceToLocation(Piece piece)
    {
        this.SetValue(Canvas.LeftProperty, piece.Location.X);
        this.SetValue(Canvas.TopProperty, piece.Location.Y);
    }

    internal void BringToTop() => this.SetValue(Canvas.ZIndexProperty, DragMovable.ZIndex);
}