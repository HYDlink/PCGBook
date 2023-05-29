namespace PCG.Maze.MazeShape;

public record GridCell(int X, int Y): CellBase
{
    public GridCell? Left { get; set; }
    public GridCell? Right { get; set; }
    public GridCell? Up { get; set; }
    public GridCell? Down { get; set; }

    public bool HasLeft => Left != null;
    public bool HasRight => Right != null;
    public bool HasUp => Up != null;
    public bool HasDown => Down != null;
    public bool HasLinkLeft => HasLeft && Links.Contains(Left);
    public bool HasLinkRight => HasRight && Links.Contains(Right);
    public bool HasLinkUp => HasUp && Links.Contains(Up);
    public bool HasLinkDown => HasDown && Links.Contains(Down);

    public override IEnumerable<GridCell> GetNeighbors() => GetNeighborsOnGridCell();
    
    public IEnumerable<GridCell> GetNeighborsOnGridCell()
    {
#pragma warning disable CS8603
        if (HasLeft)
            yield return Left;
        if (HasRight)
            yield return Right;
        if (HasUp)
            yield return Up;
        if (HasDown)
            yield return Down;
#pragma warning restore CS8603
    }

    public override IEnumerable<GridCell> GetLinks() => Links.OfType<GridCell>();

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";
}