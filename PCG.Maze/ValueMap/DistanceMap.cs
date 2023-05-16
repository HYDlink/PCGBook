using PCG.Maze.MazeShape;

namespace PCG.Maze.ValueMap;

public record DistanceMap(int Width, int Height) : Map2D(Width, Height)
{
    public Grid Grid { get; init; }
    public int MaxDist { get; private set; }

    public static DistanceMap GetDistanceMap(Grid grid, int startX, int startY)
        => GetDistanceMap(grid, grid.Cells[startY, startX]);

    public static DistanceMap GetDistanceMap(Grid grid, GridCell? startCell)
    {
        ArgumentNullException.ThrowIfNull(startCell);

        var height = grid.Height;
        var width = grid.Width;
        var distance = new DistanceMap(width, height) { Grid = grid };
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            distance[y, x] = Int32.MaxValue;

        distance[startCell] = 0;

        void Dfs(GridCell cell)
        {
            foreach (var neighbor in cell.GetLinks())
            {
                var new_dist = distance[cell] + 1;
                var formal_dist = distance[neighbor];
                if (new_dist < formal_dist)
                {
                    distance[neighbor] = new_dist;
                    Dfs(neighbor);
                }
            }
        }

        Dfs(startCell);
        distance.MaxDist = distance.values.Cast<int>().Max();

        return distance;
    }

    /// <summary>
    /// 返回一个距离图，除了从起点到达 <paramref name="endCell"/> 的路径保留了路径计算，所有其他的路径都被染上了最大值距离的距离图
    /// </summary>
    /// <param name="endCell"></param>
    /// <returns></returns>
    public DistanceMap GetPathMap(GridCell endCell)
    {
        var path_map = new DistanceMap(Width, Height) { MaxDist = MaxDist };

        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            path_map[y, x] = MaxDist;

        path_map[endCell] = this[endCell];

        var cur_cell = endCell;
        while (this[cur_cell] != 0)
        {
            var prev_path = cur_cell.GetLinks().FirstOrDefault(c => this[c] < this[cur_cell]);
            if (prev_path is null)
                break;
            path_map[prev_path] = this[prev_path];
            cur_cell = prev_path;
        }

        return path_map;
    }

    public Func<GridCell, Rgba32> GetCellColorByDistanceValue(bool newColorWithMaxDist = false)
    {
        Rgba32 GetColor(GridCell cell)
        {
            float dist_percent = (float)this[cell] / MaxDist;
            return new Rgba32(1f, 0, 1f, dist_percent);
        }

        Rgba32 GetColorSpecial(GridCell cell)
        {
            if (this[cell] == MaxDist)
                return new Rgba32(0, 0, 1f, 1f);
            return GetColor(cell);
        }

        return newColorWithMaxDist ? GetColorSpecial : GetColor;
    }
}