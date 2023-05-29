using PCG.Common;
using PCG.Maze.MazeShape;

namespace PCG.Maze;

public partial class MazeGenerator
{
    public static void KruskalLink<TCell>(IMazeMap<TCell> maze, Action<IMazeMap<TCell>>? onStepFinish = null)
        where TCell : CellBase
    {
        var random = Utilities.CreateRandomWithPrintedSeed(1);

        var cellSets = new DisjointUnion<TCell>();

        var allCells = maze.GetAllCells().ToArray();
        
        // make all existed links Union
        foreach (var cell in allCells)
        foreach (var link in cell.GetLinks().Cast<TCell>())
            cellSets.Union(cell, link);

        var allEdges = allCells
            .SelectMany(c => c.GetNeighbors().Cast<TCell>()
                .Select(neighbor => new AdjacentEdge<TCell>(c, neighbor)))
            .Distinct().ToList();

        allEdges.Shuffle(random);
        foreach (var (left, right) in allEdges)
        {
            var lSet = cellSets.Find(left);
            var rSet = cellSets.Find(right);
            if (lSet == rSet)
                continue;
            cellSets.Union(left, right);
            left.Link(right, true);
            onStepFinish?.Invoke(maze);
        }
    }

    public static void KruskalOnWeaveGrid(WeaveGrid maze, Action<IMazeMap<WeaveCell>> onStepFinish)
    {
        
    }
}