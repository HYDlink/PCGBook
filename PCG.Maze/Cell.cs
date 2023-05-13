namespace PCG.Maze;

public record class Cell(int X, int Y)
{
    public Cell? Left { get; set; }
    public Cell? Right { get; set; }
    public Cell? Up { get; set; }
    public Cell? Down { get; set; }

    public bool HasLeft => Left != null;
    public bool HasRight => Right != null;
    public bool HasUp => Up != null;
    public bool HasDown => Down != null;

    public IEnumerable<Cell> GetNeighbors()
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

    public HashSet<Cell> Links { get; } = new();

    public void Link(Cell cell, bool isBidir = false)
    {
        Links.Add(cell);
        if (isBidir) cell.Link(this);
    }
    
    public void UnLink(Cell cell, bool isBidir = false)
    {
        Links.Add(cell);
        if (isBidir) cell.UnLink(this);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public bool IsLinked(Cell cell) => Links.Contains(cell);
}