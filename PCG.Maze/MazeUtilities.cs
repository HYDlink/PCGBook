using System.Diagnostics;
using PCG.Maze.MazeShape;

namespace PCG.Maze;

public static class MazeUtilities
{
    public static IEnumerable<TCell> GetDeadEnds<TCell>(this IMazeMap<TCell> grid)
        where TCell : CellBase
        => grid.GetAllCells().Where(c => c.GetLinks().Count() == 1);

    /// <summary>
    /// 返回 <paramref name="deadEnd"/> 之前的整个死胡同路径，但不包含 <paramref name="deadEnd"/>
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="deadEnd"></param>
    /// <typeparam name="TCell"></typeparam>
    /// <returns></returns>
    public static IEnumerable<TCell> GetDeadEndCorridor<TCell>(this TCell deadEnd)
        where TCell : CellBase
    {
        Debug.Assert(deadEnd.GetLinks().Count() == 1, "deadEnd is true Dead End");
        
        var cur_cell = deadEnd.GetLinks().First();
        const int corridorLinkCount = 2;
        while (cur_cell.GetLinks().Count() == corridorLinkCount)
        {
            yield return (TCell)cur_cell;
            cur_cell = cur_cell.GetLinks().First(link => link != cur_cell);
        }
    }

}