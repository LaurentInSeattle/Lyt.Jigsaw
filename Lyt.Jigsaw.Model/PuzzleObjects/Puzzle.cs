namespace Lyt.Jigsaw.Model.PuzzleObjects;

public sealed class Puzzle
{
#if DEBUG
    public const int MaxPieceCount = 520;
#else
    public const int MaxPieceCount = 666;
#endif

    internal readonly Randomizer Randomizer;

    private readonly Profiler profiler;

    public static Dictionary<int, PuzzleSetup> GenerateSetups(IntSize imageSize)
    {
        Dictionary<int, PuzzleSetup> puzzleSetups = [];
        int minDimension = Math.Min(imageSize.Width, imageSize.Height);
        int maxPieceSize = minDimension / 3;
        for (int pieceSize = 32; pieceSize <= maxPieceSize; pieceSize += 4)
        {
            var setup = new PuzzleSetup(pieceSize, imageSize);
            int pieceCount = setup.Rows * setup.Columns;
            if (pieceCount > Puzzle.MaxPieceCount)
            {
                continue;
            }

            puzzleSetups.TryAdd(pieceCount, setup);
        }

        return puzzleSetups;
    }

#pragma warning disable CS8618 
    // Non-nullable field must contain a non-null value when exiting constructor.
    public Puzzle() { /* Required for deserialization */ }
#pragma warning restore CS8618 

    public Puzzle(ILogger logger)
    {
        this.Randomizer = new Randomizer();
        this.profiler = new Profiler(logger);
    }

    #region Serialized Properties 

    public double Background { get; set; }

    public int PieceCount { get; set; }

    public int Rows { get; set; }

    public int Columns { get; set; }

    public int PieceSize { get; set; }

    public int PieceOverlap { get; set; }

    public int RotationSteps { get; set; }

    public double PieceSnapDistance { get; set; }

    public List<Piece> Pieces { get; set; } = [];

    public List<Group> Groups { get; set; } = [];

    #endregion // Serialized Properties 

    [JsonIgnore]
    internal Dictionary<int, Piece> PieceDictionary { get; private set; } = [];

    [JsonIgnore]
    internal List<Piece> Moves { get; private set; } = [];

    public int Progress ()
    {
        int groupedPiecesCount = 
            this.Groups.Count <= 0 ? 
                0 : 
                (from grp in this.Groups select grp.Pieces.Count).Sum(); 
        double ratio = groupedPiecesCount / (double) this.PieceCount;
        return (int) Math.Round(100.0 * ratio);
    }

    public bool IsComplete
        => this.Groups.Count == 1 && this.Groups[0].Pieces.Count == this.PieceCount;

    public int ApparentPieceSize => this.PieceSize - 2 * this.PieceOverlap;

    internal int RotationStepAngle => this.RotationSteps <= 1 ? 0 : 360 / this.RotationSteps;

    public List<Piece> GetMoves() => this.Moves;

    public bool Setup(PuzzleSetup setup, int rotationSteps, int snap)
    {
        if ((rotationSteps < 0) || (rotationSteps > 6))
        {
            return false;
        }

        if ((snap < 0) || (snap > 3))
        {
            return false;
        }

        this.RotationSteps = rotationSteps;
        this.Rows = setup.Rows;
        this.Columns = setup.Columns;
        this.PieceSize = setup.PieceSize;
        this.PieceOverlap = setup.PieceSize / 4;
        this.PieceCount = this.Rows * this.Columns;
        int snapReverse = 3 - snap;
        this.PieceSnapDistance = this.PieceOverlap / 3.2 + snapReverse * this.PieceOverlap / 4.2;

        this.CreatePieces();
        return true;
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
            // Ignore self
            if (targetPiece == piece)
            {
                continue;
            }

            // Ignore invisible pieces
            if (!piece.IsVisible)
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
            //Debug.WriteLine(
            //    string.Format("Current: {0}  {1}", piece.Position.Row, piece.Position.Column));
            //Debug.WriteLine(
            //    string.Format("Target: {0}  {1}", targetPiece.Position.Row, targetPiece.Position.Column));
            //Debug.WriteLine("Distance: " + distance.ToString("F1")); 
            if (distance < minDistance)
            {
                minDistance = distance;
                double delta = Math.Abs(distance - this.PieceSize);
                if (delta > this.PieceSnapDistance)
                {
                    continue;
                }

                // Pieces should share one side.
                // Do not overwrite the placement we may have already calculated and
                // want to return. 
                if (!piece.IsSharingOneSideWith(targetPiece, out Placement sidePlacement))
                {
                    continue;
                }

                if (sidePlacement == Placement.Unknown)
                {
                    continue;
                }

                // Not enough: Should share closest sides 
                var location = piece.SnapLocation(sidePlacement);
                double moveDistance = Location.Distance(location, targetPiece.Location);
                if (moveDistance > this.PieceSnapDistance * 10)
                {
                    continue;
                }

                placement = sidePlacement;
                closestPiece = piece;
            }
        }

        return closestPiece;
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

#if DEBUG
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
#endif
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
                if (Debugger.IsAttached) { Debugger.Break(); break; }
            }
        }
    }

    internal void FinalizeAfterDeserialization()
    {
        // recreate the piece dictionary 
        foreach (var piece in this.Pieces)
        {
            this.PieceDictionary.Add(piece.Id, piece);
        }

        // finalize all pieces 
        foreach (Piece piece in this.Pieces)
        {
            piece.FinalizeAfterDeserialization(this);
        }

        // finalize all groups  
        foreach (Group group in this.Groups)
        {
            group.FinalizeAfterDeserialization(this);
        }
    }
}
