namespace Lyt.Jigsaw.Model.PuzzleObjects; 

public sealed class PuzzleImageSetup
{
    public PuzzleImageSetup(int pieceSize, IntSize imageSize)
    {
        this.PieceSize = pieceSize;
        this.Rows = imageSize.Height / pieceSize; 
        this.Columns = imageSize.Width / pieceSize;
        this.PuzzleSize = new IntSize ( this.Rows * pieceSize, this.Columns * pieceSize);
        int xOffset = (imageSize.Width - this.PuzzleSize.Width) / 2;
        int yOffset = (imageSize.Height - this.PuzzleSize.Height) / 2;
        this.PuzzleOffset = new IntPoint(xOffset, yOffset);
    }

    public int PieceSize { get; private set; }

    public IntSize PuzzleSize { get; private set; }

    public IntPoint PuzzleOffset { get; private set; }

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public int PieceCount => this.Rows * this.Columns;
}