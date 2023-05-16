using PCG.Common;
using PCG.Maze.MazeShape;

namespace PCG.Maze.Modifier;

/// <summary>
/// Braid，打通 DeadEnd 死角
/// </summary>
public static class DeadEndRemoval
{
    public static void RemoveDeadEnd<TCell>(this IMazeMap<TCell> mazeMap, float percent = 1f, int randSeed = -1)
        where TCell : CellBase
    {
        var random = Utilities.CreateRandomWithPrintedSeed(randSeed);
        foreach (var dead_end in mazeMap.GetDeadEnds())
        {
            if (random.NextSingle() > percent) continue;
            var can_dig_neighbors = dead_end.GetNeighbors().Where(n => !dead_end.IsLinked(n));
            // var to_dig = can_dig_neighbors.First();
            var to_dig = can_dig_neighbors.RandomItem(random);
            dead_end.Link(to_dig);
        }
    }

    public static void RemoveDeadEndPath<TCell>(this IMazeMap<TCell> mazeMap, float deadEndPercent = 1f,
        int randSeed = -1) where TCell : CellBase
    {
        var random = Utilities.CreateRandomWithPrintedSeed(randSeed);
        // RemoveCell 以后，迷宫又会出现新的死角，新的死角路线，所以应当拿到所有应当移除的 Cell，最后一并移除
        var deadEndPaths = mazeMap.GetDeadEnds().Where(_ => random.NextSingle() <= deadEndPercent)
            .SelectMany(deadEnd => deadEnd.GetDeadEndCorridor().Append(deadEnd))
            .ToList();
        deadEndPaths.ForEach(mazeMap.RemoveCell);
    }
}