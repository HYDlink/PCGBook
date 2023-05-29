using System.Diagnostics;
using PCG.Common;
using PCG.Maze.MazeShape;

namespace PCG.Maze;

public partial class MazeGenerator
{
    public delegate void GeneratorForGrid(Grid grid, Action<Grid> onStepFinish);

    public static void BinaryTreeLink(Grid grid, Action<Grid>? onStepFinish = null)
    {
        var random = Utilities.CreateRandomWithPrintedSeed();
        var peek_right_percent = 0.6f;

        for (var y = 0; y < grid.Height; y++)
        for (var x = 0; x < grid.Width; x++)
        {
            var cell = grid.Cells[y, x];
            if (cell.HasRight && cell.HasDown)
            {
                var peek_right = random.NextDouble() < peek_right_percent;
                if (peek_right)
                    cell.Link(cell.Right);
                else
                    cell.Link(cell.Down);
            }
            else if (cell.HasRight)
            {
                cell.Link(cell.Right);
            }
            else if (cell.HasDown)
            {
                cell.Link(cell.Down);
            }

            onStepFinish?.Invoke(grid);
        }
    }

    public static void SidewinderLink(Grid grid, Action<Grid>? onStepFinish = null)
    {
        var random = Utilities.CreateRandomWithPrintedSeed();
        var peek_right_percent = 0.6f;

        for (var y = 0; y < grid.Height; y++)
        {
            var has_down = y < grid.Height - 1;
            var is_bottom = !has_down;
            var run_start = 0;
            // peek run
            // if run failed, if has Down neighbor, peek Down
            for (int x = 0; x < grid.Width; x++)
            {
                var cell = grid.Cells[y, x];
                var is_peek_right = random.NextDouble() < peek_right_percent;
                // 最下方的水平线要全部打通
                if (cell.HasRight && (is_bottom || is_peek_right))
                {
                    cell.Link(cell.Right);
                }
                else if (has_down)
                {
                    var index_peek_down = random.Next(run_start, x + 1);
                    Debug.Assert(index_peek_down < grid.Width);
                    var to_down_cell = grid.Cells[y, index_peek_down];
                    var down = to_down_cell.Down;
                    Debug.Assert(down != null);
                    to_down_cell.Link(down, true);
                    run_start = x + 1;
                }

                onStepFinish?.Invoke(grid);
            }
        }
    }


}