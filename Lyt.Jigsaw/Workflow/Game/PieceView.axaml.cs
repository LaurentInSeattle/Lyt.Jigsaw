namespace Lyt.Jigsaw.Workflow.Game;

using Location = Model.Infrastucture.Location;

public sealed partial class PieceView : View
{
    private DragMovable? dragMovable;
    private int rotationAngle;

    public void AttachBehavior(Canvas canvas)
    {
        this.dragMovable = new DragMovable(canvas, adjustPosition: true);
        this.dragMovable.Attach(this);
        //this.Image.Effect = new DropShadowEffect
        //{
        //    Color = Colors.Black,
        //    BlurRadius = 8,
        //    Opacity = 0.5,
        //    OffsetX = 4, 
        //    OffsetY = 4, 
        //};
    }

    ~PieceView()
    {
        this.dragMovable?.Detach();
    }

    public bool HasDragMovable => this.dragMovable is not null;

    public DragMovable DragMovable 
        => this.dragMovable is not null ? 
                this.dragMovable : 
                throw new Exception("Should have cchecked HasDragMovable property");

    public Point GetCenterLocation
        => this.DataContext is PieceViewModel viewModel ? 
                viewModel.PieceCenterLocation : 
                throw new InvalidOperationException("Invalid data context");

    internal void MoveTo(Location location)
    {
        this.SetValue(Canvas.LeftProperty, location.X);
        this.SetValue(Canvas.TopProperty, location.Y);
    }

    internal void Rotate(int newRotationAngle)
    {
        if (this.rotationAngle != newRotationAngle)
        {
            this.rotationAngle = newRotationAngle;
            var transform = new RotateTransform(this.rotationAngle);
            this.Image.RenderTransform = transform;
            this.Path.RenderTransform = transform;
        }
    }

    internal void BringToTop() => this.SetValue(Canvas.ZIndexProperty, DragMovable.ZIndex);

    internal void MoveToAndRotate(Location location, int newRotationAngle, bool bringToTop)
    {
        this.SetValue(Canvas.LeftProperty, location.X);
        this.SetValue(Canvas.TopProperty, location.Y);

        if (this.rotationAngle != newRotationAngle)
        {
            this.rotationAngle = newRotationAngle;
            if (newRotationAngle == 0)
            {
                this.Image.RenderTransform = null;
                this.Path.RenderTransform = null;
            }
            else
            {
                var transform = new RotateTransform(this.rotationAngle);
                this.Image.RenderTransform = transform;
                this.Path.RenderTransform = transform;
            } 
        }

        if (bringToTop)
        {
            this.SetValue(Canvas.ZIndexProperty, DragMovable.ZIndex);
        }
    }
}