namespace PCG.Maze.MazeShape.Advanced;

public record Grid3DCell(int X, int Y, int Z) : CellBase
{
    public Grid3DCell? Left { get; set; }
    public Grid3DCell? Right { get; set; }
    public Grid3DCell? Up { get; set; }
    public Grid3DCell? Down { get; set; }
    public Grid3DCell? Forward { get; set; }
    public Grid3DCell? Backward { get; set; }

    public bool HasLeft => Left != null;
    public bool HasRight => Right != null;
    public bool HasUp => Up != null;
    public bool HasDown => Down != null;
    public bool HasForward => Forward != null;
    public bool HasBackward => Backward != null;

    public bool HasLinkLeft => HasLeft && Links.Contains(Left);
    public bool HasLinkRight => HasRight && Links.Contains(Right);
    public bool HasLinkUp => HasUp && Links.Contains(Up);
    public bool HasLinkDown => HasDown && Links.Contains(Down);
    public bool HasLinkForward => HasForward && Links.Contains(Down);
    public bool HasLinkBackward => HasBackward && Links.Contains(Down);

    public override IEnumerable<Grid3DCell> GetNeighbors() => GetNeighborsOnGridCell();

    public IEnumerable<Grid3DCell> GetNeighborsOnGridCell()
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
        if (HasForward)
            yield return Forward;
        if (HasBackward)
            yield return Backward;
#pragma warning restore CS8603
    }

    public override IEnumerable<Grid3DCell> GetLinks() => Links.OfType<Grid3DCell>();

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override string ToString() => $"({X}, {Y}, {Z})";
}