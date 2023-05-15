using PCG.Maze.MazeShape;

namespace PCG.Maze.ValueMap;

public record DistanceMap(int Width, int Height) : Map2D(Width, Height)
{
    public Grid Grid { get; init; }
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

        return distance;
    }

    public Func<GridCell, Rgba32> GetCellColorByDistanceValue()
    {
        var max_dist = Width * Height / 2;

        Rgba32 getColor(GridCell cell)
        {
            float dist_percent = (float)this[cell] / max_dist;
            return new Rgba32(1f, 0, 1f, dist_percent);
        }

        return getColor;
    }
}