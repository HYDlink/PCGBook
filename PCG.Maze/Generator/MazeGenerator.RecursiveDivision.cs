using System.Diagnostics;
using PCG.Common;
using PCG.Maze.MazeShape;

namespace PCG.Maze;

// TODO Eller 还可以用于 Circle 和 Hexagon

public partial class MazeGenerator
{
    public static void RecursiveDivision(Grid grid, Action<Grid>? onStepFinish = null)
    {
        var random = Utilities.CreateRandomWithPrintedSeed();
        grid.ConnectAllCells();
        var room_max_width = 6;
        var dig_room_percent = 0.2;

        void Divide(int x, int y, int w, int h)
        {
            if (w <= 1 || h <= 1) return;
            var can_dig_room = w < room_max_width && h < room_max_width && random.NextDouble() < dig_room_percent;
            if (can_dig_room) return;
            if (h > w) DivideVertical(x, y, w, h);
            else DivideHorizontal(x, y, w, h);
        }

        void DivideHorizontal(int x, int y, int w, int h)
        {
            // 选择墙的 x 轴位置，保证不彻底靠近边缘，留下一个空位
            var rand_x = random.Next(0, w - 1);
            var wall_x = rand_x + x;
            var wall_x_right = wall_x + 1;
            var rand_y = random.Next(h);

            // 在 x + rand_x 处，为 y 轴砌墙，并在 y + rand_y 处留下一个空位
            for (int cy = 0; cy < h; cy++)
            {
                if (cy == rand_y) continue;
                grid.Cells[y + cy, wall_x].UnLink(grid.Cells[y + cy, wall_x_right]);
            }
            
            Divide(x, y, rand_x + 1, h);
            Divide(wall_x_right, y, w - rand_x - 1, h);
        }

        void DivideVertical(int x, int y, int w, int h)
        {
            var rand_y = random.Next(0, h - 1);
            var wall_y = rand_y + y;
            var wall_y_down = wall_y + 1;
            var rand_x = random.Next(w);

            for (int cx = 0; cx < w; cx++)
            {
                if (cx == rand_x) continue;
                grid.Cells[wall_y, x + cx].UnLink(grid.Cells[wall_y_down, x + cx]);
            }
            
            Divide(x, y, w, rand_y + 1);
            Divide(x, wall_y_down, w, h - rand_y - 1);
        }
        
        Divide(0, 0, grid.Width, grid.Height);
    }
}
