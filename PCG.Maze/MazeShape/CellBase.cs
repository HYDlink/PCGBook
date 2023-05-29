namespace PCG.Maze.MazeShape;

/// <summary>
/// 注，所有继承 CellBase 的，如果自己添加了邻居字段，比如 <see cref="GridCell.Left"/>，要自己重载 GetHashCode()
/// 否则自动生成的 GetHashCode() 会为这个邻居（比如 Left）也计算 HashCode，而 Left 的自己字段 Right 又会参与 GetHashCode 检查，
/// 最后导致递归爆栈
/// </summary>
public abstract record CellBase
{
    public abstract IEnumerable<CellBase> GetNeighbors();
    public virtual IEnumerable<CellBase> GetLinks() => Links;
    protected HashSet<CellBase> Links { get; } = new();

    public virtual void Link(CellBase cell, bool isBidir = false)
    {
        Links.Add(cell);
        if (isBidir) cell.Link(this);
    }
    
    public void UnLink(CellBase cell, bool isBidir = false)
    {
        Links.Remove(cell);
        if (isBidir) cell.UnLink(this);
    }

    public bool IsLinked(CellBase? cell) => cell != null && Links.Contains(cell);
    public bool IsDeadEnd => Links.Count == 1;
}