namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Puzzle
{
    private readonly Dictionary<int, PuzzleSetup> puzzleSetups;

    internal readonly Randomizer Randomizer;

    private readonly Profiler profiler;

    public Puzzle(ILogger logger, int height, int width, int rotationSteps)
    {
        this.ImageSize = new(height, width);
        this.RotationSteps = rotationSteps;
        this.Randomizer = new Randomizer();
        this.profiler = new Profiler(logger);
        this.puzzleSetups = [];
        this.PieceSnapDistance = 20.0;
        this.GenerateSetups();
    }

    public List<int> PieceCounts => [.. this.puzzleSetups.Keys];

    public List<Piece> Pieces { get; private set; } = [];

    public int PieceCount { get; private set; }

    public bool IsComplete
        => this.Groups.Count == 1 && this.Groups[0].Pieces.Count == this.PieceCount;

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public int PieceSize { get; set; }

    public int PieceOverlap { get; private set; }

    public int ApparentPieceSize => this.PieceSize - 2 * this.PieceOverlap;

    internal int RotationSteps { get; private set; }

    internal int RotationStepAngle { get; private set; }

    internal List<Group> Groups { get; private set; } = [];

    internal Dictionary<int, Piece> PieceDictionary { get; private set; } = [];

    internal List<Piece> Moves { get; private set; } = [];

    private IntSize ImageSize { get; set; }

    private double PieceSnapDistance { get; set; }

    public List<Piece> GetMoves() => this.Moves;

    public bool Setup(int pieceCount, int rotationSteps)
    {
        if ((rotationSteps < 0) || (rotationSteps > 6))
        {
            return false;
        }

        if (!this.puzzleSetups.TryGetValue(pieceCount, out var setup) || setup is null)
        {
            return false;
        }

        this.Rows = setup.Rows;
        this.Columns = setup.Columns;
        this.PieceSize = setup.PieceSize;
        this.PieceOverlap = setup.PieceSize / 4;
        this.PieceCount = pieceCount;
        if (rotationSteps == 0)
        {
            this.RotationStepAngle = 0;
        }
        else
        {
            this.RotationStepAngle = 360 / rotationSteps;
        }

        this.CreatePieces();
        return true;
    }

    public void Save()
    {
        // TODO 
        Debug.WriteLine("Save: TODO!");
        // Serialize and save to disk 
        Debug.WriteLine("Saved");
    }

    public bool CheckForSnaps(Piece movingPiece)
    {
        this.Moves.Clear();

        // Find closest piece
        Piece? closest = this.FindCloseTo(movingPiece, out Placement placement);
        if (closest is null)
        {
            if (movingPiece.IsGrouped)
            {
                foreach (var groupPiece in movingPiece.Group.Pieces)
                {
                    if (groupPiece == movingPiece)
                    {
                        continue;
                    }

                    closest = this.FindCloseTo(groupPiece, out placement);
                    if (closest is not null)
                    {
                        if (placement == Placement.Unknown)
                        {
                            // Bug: this should never happen ! 
                            // If we have found a piece we MUST have a placement 
                            if (Debugger.IsAttached) { Debugger.Break(); }

                            // Prevent the crash on invoking placement.Opposite()
                            return false;
                        }

                        // var oldLocation = closest.Location;
                        groupPiece.SnapTargetToThis(closest, placement.Opposite());
                        if (closest.IsGrouped)
                        {
                            // Moving the piece when snapping will adjust the group as well 
                            foreach (var closestGroupPiece in closest.Group.Pieces)
                            {
                                if (closestGroupPiece == closest)
                                {
                                    continue;
                                }

                                this.Moves.Add(closestGroupPiece);
                            }
                        }

                        closest.ManageGroups(groupPiece);
                        break;
                    }
                }
            }
        }
        else
        {
            // Found 
            if (placement == Placement.Unknown)
            {
                // Bug: this should never happen ! 
                // If we have found a piece we MUST have a placement 
                if (Debugger.IsAttached) { Debugger.Break(); }

                // Prevent the crash on invoking placement.Opposite()
                return false;
            }

            if (movingPiece.IsGrouped)
            {
                movingPiece.SnapTargetToThis(closest, placement.Opposite());
                if (closest.IsGrouped)
                {
                    // Moving the piece when snapping will adjust the group as well 
                    foreach (var closestGroupPiece in closest.Group.Pieces)
                    {
                        if (closestGroupPiece == closest)
                        {
                            continue;
                        }

                        this.Moves.Add(closestGroupPiece);
                    }
                }
            }
            else
            {
                closest.SnapTargetToThis(movingPiece, placement);
            }

            closest.ManageGroups(movingPiece);
        }

        return this.Moves.Count > 0;
    }

    private Piece? FindCloseTo(Piece targetPiece, out Placement placement)
    {
        placement = Placement.Unknown;
        double minDistance = double.MaxValue;
        Piece? closestPiece = null;
        Group? targetPieceGroup = targetPiece.MaybeGroup;
        foreach (Piece piece in this.Pieces)
        {
            if (targetPiece == piece)
            {
                continue;
            }

            // pieces should have the same orientation 
            if (piece.RotationAngle != targetPiece.RotationAngle)
            {
                continue;
            }

            // pieces should not belong to the same group is there is one 
            if ((targetPieceGroup is not null) && (piece.MaybeGroup is Group pieceGroup))
            {
                if (targetPieceGroup == pieceGroup)
                {
                    continue;
                }
            }

            // distance should be equal to piece size + or - the snap visual distance 
            double distance = Location.Distance(piece.Center, targetPiece.Center);
            if (distance < minDistance)
            {
                minDistance = distance;
                double delta = Math.Abs(distance - this.PieceSize);
                if (delta > this.PieceSnapDistance)
                {
                    continue;
                }

                // pieces should share one side  
                if (!piece.IsSharingOneSideWith(targetPiece, out placement))
                {
                    continue;
                }

                // Not enough: Should share closest sides 
                var location = piece.SnapLocation(placement);
                double moveDistance = Location.Distance(location, targetPiece.Location);
                if (moveDistance > this.PieceSnapDistance * 10)
                {
                    continue;
                }

                closestPiece = piece;
            }
        }

        return closestPiece;
    }

    private void GenerateSetups()
    {
        int minDimension = Math.Min(this.ImageSize.Width, this.ImageSize.Height);
        int maxPieceSize = minDimension / 3;
        for (int pieceSize = 32; pieceSize <= maxPieceSize; pieceSize += 4)
        {
            var setup = new PuzzleSetup(pieceSize, this.ImageSize);
            int pieceCount = setup.Rows * setup.Columns;
            this.puzzleSetups.TryAdd(pieceCount, setup);
        }
    }

    private void CreatePieces()
    {
        this.profiler.StartTiming();

        for (int row = 0; row < this.Rows; ++row)
        {
            for (int col = 0; col < this.Columns; ++col)
            {
                var piece = new Piece(this, row, col);
                this.Pieces.Add(piece);
                this.PieceDictionary.Add(piece.Id, piece);
            }
        }

        for (int row = 0; row < this.Rows; ++row)
        {
            for (int col = 0; col < this.Columns; ++col)
            {
                var piece = this.FromId(this.ToId(row, col));
                piece.UpdateSides();
            }
        }

        // Measured at less of 12 ms in debug build for 180 pieces 
        // Measured at less of 35 ms in debug build for 1920 pieces 
        this.profiler.EndTiming(string.Format("Creating point lists - Piece Count: {0}", this.PieceCount));

        for (int row = 0; row < this.Rows; ++row)
        {
            for (int col = 0; col < this.Columns; ++col)
            {
                var piece = this.FromId(this.ToId(row, col));
                if (piece.AnySideUnknown)
                {
                    throw new Exception("Failed to calculate sides");
                }
            }
        }
    }

    private Piece FromId(int id)
        => this.PieceDictionary.TryGetValue(id, out Piece? piece) && piece is not null ?
                piece :
                throw new ArgumentException("No such Piece Id ");

    public void VerifyLostPieces(double canvasWidth, double canvasHeight)
    {
        foreach (Piece piece in this.Pieces)
        {
            var location = piece.Location;
            if (location.X > canvasWidth ||
                location.Y > canvasHeight ||
                location.X < -this.PieceSize ||
                location.Y < -this.PieceSize)
            {
                if (Debugger.IsAttached) { Debugger.Break(); break;  }
            }
        }
    }
}
