namespace PCG.Maze;

public record DistanceMap(int Width, int Height)
{
    private int[,] values = new int[Height, Width];
    public Grid Grid { get; init; }

    public int this[int y, int x]
    {
        get => values[y, x];
        set => values[y, x] = value;
    }

    public int this[Cell cell]
    {
        get => values[cell.Y, cell.X];
        set => values[cell.Y, cell.X] = value;
    }

    public static DistanceMap GetDistanceMap(Grid grid, int startX, int startY)
    {
        var start_cell = grid.Cells[startY, startX];
        var height = grid.Height;
        var width = grid.Width;
        var distance = new DistanceMap(width, height) { Grid = grid };
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            distance[y, x] = Int32.MaxValue;

        distance[start_cell] = 0;

        void Dfs(Cell cell)
        {
            foreach (var neighbor in cell.Links)
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

        Dfs(start_cell);

        return distance;
    }
    
    public Func<Cell, Rgba32> GetCellColorByDistanceValue()
    {
        var max_dist = Width * Height / 2;

        Rgba32 getColor(Cell cell)
        {
            float dist_percent = (float)this[cell] / max_dist;
            return new Rgba32(1f, 0, 1f, dist_percent);
        }

        return getColor;
    }
}