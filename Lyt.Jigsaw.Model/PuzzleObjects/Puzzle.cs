namespace Lyt.Jigsaw.Model.PuzzleObjects;

using System.Collections.Generic;

public sealed class Puzzle
{
#if DEBUG
    public const int MaxPieceCount = 1080;
#else
    public const int MaxPieceCount = 1080;
#endif

    internal Randomizer Randomizer;

    // TODO: Need to get a profile after deserialization as well
    private Profiler profiler;

    public static Dictionary<int, PuzzleImageSetup> GenerateSetups(IntSize imageSize)
    {
        Dictionary<int, PuzzleImageSetup> puzzleSetups = [];
        int minDimension = Math.Min(imageSize.Width, imageSize.Height);
        int maxPieceSize = minDimension / 3;
        for (int pieceSize = 32; pieceSize <= maxPieceSize; pieceSize += 4)
        {
            var setup = new PuzzleImageSetup(pieceSize, imageSize);
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

    // 1.7: Lazily accomodate for piece alignment and spacing 
    private Location Center => 
        new (this.PieceSize * this.Columns / 1.7, this.PieceSize * this.Rows / 1.7);

    internal int Progress()
    {
        int groupedPiecesCount =
            this.Groups.Count <= 0 ?
                0 :
                (from grp in this.Groups select grp.Pieces.Count).Sum();
        double ratio = groupedPiecesCount / (double)this.PieceCount;
        int percent = (int)Math.Round(100.0 * ratio);
        if (percent == 100 && !this.IsComplete)
        {
            percent = 99;
        }

        return percent;
    }

    public bool IsComplete
        => this.Groups.Count == 1 && this.Groups[0].Pieces.Count == this.PieceCount;

    public int ApparentPieceSize => this.PieceSize - 2 * this.PieceOverlap;

    internal int RotationStepAngle => this.RotationSteps <= 1 ? 0 : 360 / this.RotationSteps;

    internal List<Piece> GetMoves() => this.Moves;

    internal bool Setup(PuzzleImageSetup setup, PuzzleParameters puzzleParameters)
    {
        if ((puzzleParameters.RotationSteps < 0) || (puzzleParameters.RotationSteps > 6))
        {
            return false;
        }

        if ((puzzleParameters.Snap < 0) || (puzzleParameters.Snap > 3))
        {
            return false;
        }

        this.RotationSteps = puzzleParameters.RotationSteps;
        this.Rows = setup.Rows;
        this.Columns = setup.Columns;
        this.PieceSize = setup.PieceSize;
        this.PieceOverlap = setup.PieceSize / 4;
        this.PieceCount = this.Rows * this.Columns;
        int snapReverse = 3 - puzzleParameters.Snap;
        double maybeSnapDistance = this.PieceOverlap / 3.2 + snapReverse * this.PieceOverlap / 4.2;
        this.PieceSnapDistance = Math.Max(10.0, maybeSnapDistance);

        this.CreatePieces();
        return true;
    }

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

    internal void FinalizeAfterDeserialization(ILogger logger)
    {
        this.Randomizer = new Randomizer();
        this.profiler = new Profiler(logger);

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

    internal bool CheckForSnaps(Piece movingPiece)
    {
        this.Moves.Clear();

        if (movingPiece.IsGrouped)
        {
            // Duplicate the list to avoid exception caused by modification during iteration
            var piecesToCheck = movingPiece.Group.Pieces.ToList();
            Piece? pieceToSnapTo = null;
            foreach (var groupPiece in piecesToCheck)
            {
                var candidates = this.GetSnapNeighboursOf(groupPiece);
                if (candidates.Count == 0)
                {
                    continue;
                }

                // If more than one candidates are valid, pick the first one
                SnapPiece snapPiece = candidates[0];
                pieceToSnapTo = snapPiece.Piece;
                pieceToSnapTo.SnapTargetToThis(groupPiece, snapPiece.Placement.Opposite());
                pieceToSnapTo.ManageGroups(groupPiece);

                this.Moves.Add(pieceToSnapTo);

                break;
            }

            if (pieceToSnapTo is not null)
            {
                // Moving the piece when snapping will adjust the group as well 
                // but we need to mark them as moved so that the UI will update accordingly 
                foreach (var groupPiece in piecesToCheck)
                {
                    if (groupPiece == pieceToSnapTo)
                    {
                        continue;
                    }

                    this.Moves.Add(groupPiece);
                }
            }
        }
        else
        {
            var candidates = this.GetSnapNeighboursOf(movingPiece);
            if (candidates.Count == 0)
            {
                return false;
            }

            // If more than one candidates are valid, pick the first one because they are sorted by distance
            SnapPiece snapPiece = candidates[0];
            Piece pieceToSnapTo = snapPiece.Piece;

            // Here we snap the moving piece only, the group (if any) will stay put 
            this.Moves.Add(movingPiece);
            pieceToSnapTo.SnapTargetToThis(movingPiece, snapPiece.Placement.Opposite());
            pieceToSnapTo.ManageGroups(movingPiece);
        }

        return this.Moves.Count > 0;
    }

    private List<SnapPiece> GetSnapNeighboursOf(Piece movingPiece)
    {
        List<SnapPiece> neighbours = [];
        Group? pieceGroup = movingPiece.MaybeGroup;
        Piece candidate;

        bool IsValidCandidate()
        {
            // Ignore invisible pieces
            if (!candidate.IsVisible)
            {
                return false;
            }

            // pieces should have the same orientation 
            if (movingPiece.RotationAngle != candidate.RotationAngle)
            {
                return false;
            }

            // pieces should not belong to the same group is there is one 
            if ((pieceGroup is not null) && (candidate.MaybeGroup is Group candidateGroup))
            {
                if (pieceGroup == candidateGroup)
                {
                    return false;
                }
            }

            // distance should be equal to piece size + or - the snap visual distance 
            double distance = Location.Distance(movingPiece.Center, candidate.Center);
            double delta = Math.Abs(distance - this.PieceSize);
            if (delta > this.PieceSnapDistance)
            {
                return false;
            }

            return true;
        }

        if (!movingPiece.IsTop)
        {
            candidate = movingPiece.GetTop();
            if (IsValidCandidate())
            {
                double distance = Location.Distance(movingPiece.CenterTop, candidate.CenterBottom);
                if (distance < this.PieceSnapDistance)
                {
                    neighbours.Add(new SnapPiece(candidate, Placement.Top, distance));
                }
            }
        }

        if (!movingPiece.IsBottom)
        {
            candidate = movingPiece.GetBottom();
            if (IsValidCandidate())
            {
                double distance = Location.Distance(movingPiece.CenterBottom, candidate.CenterTop);
                if (distance < this.PieceSnapDistance)
                {
                    neighbours.Add(new SnapPiece(candidate, Placement.Bottom, distance));
                }
            }
        }

        if (!movingPiece.IsLeft)
        {
            candidate = movingPiece.GetLeft();
            if (IsValidCandidate())
            {
                double distance = Location.Distance(movingPiece.CenterLeft, candidate.CenterRight);
                if (distance < this.PieceSnapDistance)
                {
                    neighbours.Add(new SnapPiece(candidate, Placement.Left, distance));
                }
            }
        }

        if (!movingPiece.IsRight)
        {
            candidate = movingPiece.GetRight();
            if (IsValidCandidate())
            {
                double distance = Location.Distance(movingPiece.CenterRight, candidate.CenterLeft);
                if (distance < this.PieceSnapDistance)
                {
                    neighbours.Add(new SnapPiece(candidate, Placement.Right, distance));
                }
            }
        }

        if (neighbours.Count >= 2)
        {
            neighbours.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        return neighbours;
    }

    internal bool ProvideHint()
    {
        if (this.IsComplete)
        {
            return false;
        }

        this.profiler.StartTiming();

        // How many pieces should be hinted at once
        int hintPieceCount = this.PieceCount / 32;
        hintPieceCount = Math.Max(2, hintPieceCount);
        hintPieceCount = Math.Min(16, hintPieceCount);

        bool TryHint ()
        {
            var ungroupedPieces = (from piece in this.Pieces where !piece.IsGrouped select piece).ToList();
            if (ungroupedPieces.Count == 0)
            {
                return false;
            }

            int retries = 0;
            List<SnapPiece> hintedPieces = [];
            while (retries < 20)
            {
                hintedPieces.Clear();
                int randomIndex = this.Randomizer.Next(0, ungroupedPieces.Count);
                var startingPiece = ungroupedPieces[randomIndex];
                // Debug.WriteLine("Starting Piece: " + startingPiece.Position.Row + " " + startingPiece.Position.Column);
                hintedPieces.Add(new SnapPiece(startingPiece, Placement.Left, 0.0));

                bool loopSuccess = true;
                while (hintedPieces.Count < hintPieceCount)
                {
                    var lastHintedPiece = hintedPieces[^1];
                    var snapCandidates = lastHintedPiece.Piece.GetUngroupedNeighbours(hintedPieces);
                    if (snapCandidates.Count == 0)
                    {
                        loopSuccess = false;
                        break;
                    }

                    // Snap the last hinted piece to the first candidate
                    int randomSnapIndex = this.Randomizer.Next(0, snapCandidates.Count);
                    SnapPiece snapPiece = snapCandidates[randomSnapIndex];
                    // Debug.WriteLine("Snap Piece: " + snapPiece.Piece.Position.Row + " " + snapPiece.Piece.Position.Column);
                    if (snapPiece.Piece.IsGrouped)
                    {
                        loopSuccess = false;
                        break;
                    }

                    hintedPieces.Add(snapPiece);
                }

                if (!loopSuccess)
                {
                    ++retries;
                    continue;
                }

                // Apply the hints
                // remove the starting piece
                hintedPieces.RemoveAt(0);
                startingPiece.ResetRotation();

                Piece lastSnap = startingPiece;
                foreach (var snapPiece in hintedPieces)
                {
                    var pieceToSnapTo = snapPiece.Piece;

                    this.Moves.Add(startingPiece);
                    pieceToSnapTo.ResetRotation();
                    var targetPlacement = snapPiece.Placement.Opposite();
                    pieceToSnapTo.SnapTargetToThis(startingPiece, snapPiece.Placement.Opposite());
                    pieceToSnapTo.ManageGroups(startingPiece);

                    startingPiece = pieceToSnapTo;
                    lastSnap = pieceToSnapTo;
                }

                this.Moves.Add(lastSnap);
                lastSnap.MoveTo(this.Center.X, this.Center.Y);

                // Success
                return true;
            }

            return false;
        } 

        while (hintPieceCount >= 2)
        {
            if (TryHint())
            {
                this.profiler.EndTiming("Hint Success");
                return true;
            }

            --hintPieceCount;
        }

        this.profiler.EndTiming("Hint Failed");
        return false;
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
}
