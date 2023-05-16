namespace PCG.Maze.MazeShape;

public record CircleCell(int PolarIndex, int RingIndex): CellBase
{
    #region Neighbors
    // Tips：命名不规范，没有以 Neighbor 结尾，如果这样写多了其他用户很难读懂
    
    public CircleCell? CounterClock { get; set; }
    public CircleCell? Clock { get; set; }
    // public CircleCell? Outer { get; set; }
    public CircleCell? Inner { get; set; }
    /// <summary>
    /// 应对外环的 subdivision 的多邻居问题，不再采用单一的 Outer neighbor 存储
    /// </summary>
    public List<CircleCell> OuterNeighbors { get; } = new();

    public bool HasCounterClock => CounterClock != null;
    public bool HasClock => Clock != null;
    // public bool HasOuter => Outer != null;
    public bool HasInner => Inner != null;
    public bool HasLinkCounterClock => HasCounterClock && Links.Contains(CounterClock);
    public bool HasLinkClock => HasClock && Links.Contains(Clock);
    // public bool HasLinkOuter => HasOuter && Links.Contains(Outer);
    public bool HasLinkInner => HasInner && Links.Contains(Inner);

    public override IEnumerable<CircleCell> GetNeighbors()
    {
#pragma warning disable CS8603
        if (HasCounterClock)
            yield return CounterClock;
        if (HasClock)
            yield return Clock;
        // if (HasOuter)
        //     yield return Outer;
        if (HasInner)
            yield return Inner;
        foreach (var other_neighbor in OuterNeighbors)
        {
            yield return other_neighbor;
        }
#pragma warning restore CS8603
    }

    #endregion

    public override int GetHashCode()
        => HashCode.Combine(PolarIndex, RingIndex);
}