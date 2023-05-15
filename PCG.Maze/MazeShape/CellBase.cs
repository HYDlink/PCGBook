namespace PCG.Maze.MazeShape;

public abstract record CellBase
{
    public abstract IEnumerable<CellBase> GetNeighbors();
    public virtual IEnumerable<CellBase> GetLinks() => Links;
    protected HashSet<CellBase> Links { get; } = new();

    public void Link(CellBase cell, bool isBidir = false)
    {
        Links.Add(cell);
        if (isBidir) cell.Link(this);
    }
    
    public void UnLink(CellBase cell, bool isBidir = false)
    {
        Links.Remove(cell);
        if (isBidir) cell.UnLink(this);
    }

    public bool IsLinked(CellBase cell) => Links.Contains(cell);
    public bool IsDeadEnd => Links.Count == 1;
}