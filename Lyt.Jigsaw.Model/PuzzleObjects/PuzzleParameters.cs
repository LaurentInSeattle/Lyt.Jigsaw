namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class PuzzleParameters
{
    public PuzzleParameters() {  /* for serialization */ }

    public PuzzleParameters(PuzzleImageSetup setup, int rotationSteps, int snap, int hints)
    {
        this.PieceCount = setup.Rows * setup.Columns;
        this.Rows = setup.Rows;
        this.Columns = setup.Columns;
        this.RotationSteps = rotationSteps;
        this.Snap = snap;
        this.Hints = hints;
    }

    #region Serialized Properties ( Must all be public for both get and set ) 

    public int PieceCount { get; set; }

    public int Rows { get; set; }

    public int Columns { get; set; }

    public int RotationSteps { get; set; }

    public int Snap { get; set; }

    public int Hints { get; set; }

    #endregion Serialized Properties ( Must all be public for both get and set ) 
}
