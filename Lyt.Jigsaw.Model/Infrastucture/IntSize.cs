namespace Lyt.Jigsaw.Model.Infrastucture; 

public struct IntSize
{
    public IntSize(int height, int width)
    {
        this.Height = height; 
        this.Width = width;
    }

    public int Height { get; set; }

    public int Width { get; set; }
}

