namespace PCG.Maze.MazeShape;

public record HexCell(int X, int Y) : CellBase
{
    public HexCell? LT { get; set; }
    public HexCell? RT { get; set; }
    public HexCell? Left { get; set; }
    public HexCell? Right { get; set; }
    public HexCell? LB { get; set; }
    public HexCell? RB { get; set; }
    
    public bool HasLT => LT != null;
    public bool HasRT => RT != null;
    public bool HasLeft => Left != null;
    public bool HasRight => Right != null;
    public bool HasLB => LB != null;
    public bool HasRB => RB != null;
    
    public bool HasLinkedLT => HasLT && IsLinked(LT);
    public bool HasLinkedRT => HasRT && IsLinked(RT);
    public bool HasLinkedLeft => HasLeft && IsLinked(Left);
    public bool HasLinkedRight => HasRight && IsLinked(Right);
    public bool HasLinkedLB => HasLB && IsLinked(LB);
    public bool HasLinkedRB => HasRB && IsLinked(RB);
    public override IEnumerable<CellBase> GetNeighbors()
    {
        if (HasLT) yield return LT;
        if (HasRT) yield return RT;
        if (HasLeft) yield return Left;
        if (HasRight) yield return Right;
        if (HasLB) yield return LB;
        if (HasRB) yield return RB;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";
}