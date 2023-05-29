using System.Diagnostics;

namespace PCG.Maze.MazeShape;

public abstract record WeaveCell(int X, int Y) : GridCell(X, Y)
{
    public bool IsHorizontalPassage => IsLinked(Left) && IsLinked(Right);
    public bool IsVerticalPassage => IsLinked(Up) && IsLinked(Down);
}

public record OverCell(WeaveGrid WeaveGrid, int X, int Y) : WeaveCell(X, Y)
{
    public override void Link(CellBase cell, bool isBidir = false)
    {
        if (cell is WeaveCell weaveCell)
        {
            GridCell tunnel_under = null;
            if (weaveCell.Left == Right) tunnel_under = Right;
            if (weaveCell.Right == Left) tunnel_under = Left;
            if (weaveCell.Up == Down) tunnel_under = Down;
            if (weaveCell.Down == Up) tunnel_under = Up;

            if (tunnel_under is OverCell tunnelUnder)
            {
                WeaveGrid.TunnelUnder(tunnelUnder);
                return;
            }
        }

        base.Link(cell, isBidir);
    }

    public override IEnumerable<GridCell> GetNeighbors()
    {
        foreach (var neighbor in GetNeighborsOnGridCell())
            yield return neighbor;

        if (Left is WeaveCell { IsVerticalPassage: true } && Left.Left is WeaveCell left_weave)
            yield return left_weave;
        if (Right is WeaveCell { IsVerticalPassage: true } && Right.Right is WeaveCell right_weave)
            yield return right_weave;
        if (Up is WeaveCell { IsVerticalPassage: true } && Up.Up is WeaveCell up_weave)
            yield return up_weave;
        if (Down is WeaveCell { IsVerticalPassage: true } && Down.Down is WeaveCell down_weave)
            yield return down_weave;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"O ({X}, {Y})";
}

/// <summary>
/// 不会自动连接到相隔一个格子的远处的 WeaveCell，不会自己 Tunnel，
/// 用于 <see cref="MazeGenerator.KruskalLink"/>，让 Kruskal 算法能够单纯的连接邻居而不是产生新的 Tunnel
/// </summary>
/// <param name="WeaveGrid"></param>
/// <param name="X"></param>
/// <param name="Y"></param>
public record SimpleOverCell(WeaveGrid WeaveGrid, int X, int Y) : OverCell(WeaveGrid, X, Y)
{
    public override void Link(CellBase cell, bool isBidir = false)
    {
        Links.Add(cell);
        if (isBidir) cell.Link(this);
    }

    public override IEnumerable<GridCell> GetNeighbors()
    {
        if (IsVerticalPassage)
        {
            yield return Up;
            yield return Down;
        }
        else if (IsHorizontalPassage)
        {
            yield return Left;
            yield return Right;
        }
        else
        {
            foreach (var neighbor in GetNeighborsOnGridCell())
                yield return neighbor;
        }
    }
}

public record UnderCell(OverCell OverCell, int X, int Y) : WeaveCell(X, Y)
{
    /// <summary>
    /// 给 <see cref="WeaveGrid"/> 挖掘用
    /// </summary>
    /// <param name="overCell"></param>
    public UnderCell(OverCell overCell) : this(overCell, overCell.X, overCell.Y)
    {
        OverCell = overCell;
        // 如果上面是水平的道路，那么就连接垂直的邻居，反之亦然
        if (overCell.IsHorizontalPassage)
        {
            Up = overCell.Up;
            Down = overCell.Down;
            Debug.Assert(Up != null && Down != null, "挖掘方向上下一定有邻居");
            Up.Down = this;
            Down.Up = this;
            Link(Up, true);
            Link(Down, true);
        }
        else
        {
            Debug.Assert(overCell.IsVerticalPassage);
            Left = overCell.Left;
            Right = overCell.Right;
            Debug.Assert(Left != null && Right != null, "挖掘方向左右一定有邻居");
            Left.Right = this;
            Right.Left = this;
            Link(Left, true);
            Link(Right, true);
        }
    }

    public override IEnumerable<GridCell> GetNeighbors()
    {
        if (IsVerticalPassage)
        {
            yield return Up;
            yield return Down;
        }
        else if (IsHorizontalPassage)
        {
            yield return Left;
            yield return Right;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public override int GetHashCode() => HashCode.Combine(OverCell.GetHashCode(), X, Y);
    public override string ToString() => $"U ({X}, {Y})";
}