using System.Diagnostics;
using PCG.Common;
using PCG.Maze.MazeShape;

namespace PCG.Maze;

// TODO Eller 还可以用于 Circle 和 Hexagon

public partial class MazeGenerator
{
    public static void EllerOnGrid(Grid grid, Action<Grid>? onStepFinish = null)
    {
        var random = Utilities.CreateRandomWithPrintedSeed();
        var cellSets = new DisjointUnion<GridCell>();
        var link_adjacent_chance = 0.5;

        for (int y = 0; y < grid.Height - 1; y++)
        {
            var last_linked_x = -1;

            // endX: included
            // 如果在 sub row 都只选择一个 Cell 向下挖掘的话，最后必然每个 sub row 都在不同的 set 中，放到最下面一行，
            // 要把所有不同 set 的 sub row 连接起来的时候，就会把一整行联通
            // 因此对每个 sub row 应该随机选取多个 Cell 向下挖掘
            void CarveDownInRange(int startX, int endX)
            {
                var max_count = endX - startX + 1;
                // 不要选取太多的 Cell 向下挖掘，否则会出现，到了第三行开始就直线向下挖的情况
                var max_rand_count = Math.Max(1, (max_count + 1) / 2);
                
                Debug.Assert(max_count >= 1);
                foreach (var carve_down_x in Enumerable.Range(startX, max_count)
                             .OrderBy(_ => random.Next())
                             .Take(random.Next(1, max_rand_count)))
                {
                    var carve_cell = grid.Cells[y, carve_down_x];
                    if (carve_cell is { Down: { } down_cell })
                    {
                        carve_cell.Link(down_cell, true);
                        cellSets.Union(carve_cell, down_cell);
                    }
                    else throw new InvalidOperationException();
                }
                // onStepFinish?.Invoke(grid);
            }

            // 不断尝试往右邻居链接
            for (int x = 0; x < grid.Width - 1; x++)
            {
                var cell = grid.Cells[y, x];
                if (cell is null) continue;
                if (cell.HasRight
                    && cellSets.Find(cell) != cellSets.Find(cell.Right)
                    && random.NextDouble() < link_adjacent_chance)
                {
                    cell.Link(cell.Right, true);
                    cellSets.Union(cell, cell.Right);
                }
                else
                {
                    onStepFinish?.Invoke(grid);
                    CarveDownInRange(last_linked_x + 1, x);
                    last_linked_x = x;
                }
            }

            // 最后的 subRow 还没处理
            CarveDownInRange(last_linked_x + 1, grid.Width - 1);
            // choose at least one cell in linked sub row to carve down
            onStepFinish?.Invoke(grid);
        }

        // for last row, link all neighbor that is not ine one set
        var last_row = grid.Height - 1;
        for (int x = 0; x < grid.Width - 1; x++)
        {
            var cell = grid.Cells[last_row, x];
            if (cell is null) continue;
            if (cell.HasRight && cellSets.Find(cell) != cellSets.Find(cell.Right))
            {
                cell.Link(cell.Right, true);
                cellSets.Union(cell, cell.Right);
            }
        }

        onStepFinish?.Invoke(grid);
    }
}