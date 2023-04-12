public enum CaveCell
{
    Empty,
    Stone,
    Wall
}

public class CaveCA
{
    public double ToWallRatio { get; set; } = 0.4;
    public int Width { get; set; }
    public int Height { get; set; }
    public double CavePercent { get; set; }
    public int ExecuteTimes { get; set; }
    public CaveCell[,] Map { get; set; }

    private Random Random { get; set; } = new Random();
    public int Seed { get; set; }

    public CaveCA(int width, int height, double cavePercent, int executeTimes = 2)
    {
        Width = width;
        Height = height;
        CavePercent = cavePercent;
        ExecuteTimes = executeTimes;
    }

    public void Execute()
    {
        Initialize();
        for (int i = 0; i < ExecuteTimes; i++)
        {
            Automation();
        }

        DrawEdge();
    }

    public void Initialize()
    {
        // Seed = Random.Next();
        Random = new Random(Seed);
        Console.WriteLine("The random seed is: " + Seed);

        Map = new CaveCell[Height, Width];

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
            Map[y, x] = Random.NextDouble() < CavePercent
                ? CaveCell.Stone
                : CaveCell.Empty;
    }

    public IEnumerable<CaveCell> GetAllNeighbors(int x, int y)
    {
        if (x > 0)
        {
            var nx = x - 1;
            yield return Map[y, nx];
            if (y > 0)
                yield return Map[y - 1, nx];
            if (y < Height - 1)
                yield return Map[y + 1, nx];
        }

        if (y > 0)
            yield return Map[y - 1, x];
        if (y < Height - 1)
            yield return Map[y + 1, x];

        if (x < Width - 1)
        {
            var nx = x + 1;
            yield return Map[y, nx];
            if (y > 0)
                yield return Map[y - 1, nx];
            if (y < Height - 1)
                yield return Map[y + 1, nx];
        }
    }

    public IEnumerable<CaveCell> GetAllNeighborsInCross(int x, int y)
    {
        if (x > 0)
        {
            var nx = x - 1;
            yield return Map[y, nx];
        }

        if (y > 0)
            yield return Map[y - 1, x];
        if (y < Height - 1)
            yield return Map[y + 1, x];

        if (x < Width - 1)
        {
            var nx = x + 1;
            yield return Map[y, nx];
        }
    }

    public int GetNeighborCountWhere(int x, int y, Func<CaveCell, bool> predicate)
        => GetAllNeighbors(x, y).Count(predicate);

    public void Automation()
    {
        var newMap = new CaveCell[Height, Width];

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var all_neighbors = GetAllNeighbors(x, y);
            var be_stone = (all_neighbors.Count(c => c == CaveCell.Stone) / (double)all_neighbors.Count()) > ToWallRatio;
            // var stone_count = GetNeighborCountWhere(x, y, c => c == CaveCell.Stone);
            newMap[y, x] = be_stone ? CaveCell.Stone : CaveCell.Empty;
        }

        Map = newMap;
    }

    public void DrawEdge()
    {
        var newMap = new CaveCell[Height, Width];

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var be_wall = Map[y, x] == CaveCell.Stone
                          && GetAllNeighborsInCross(x, y).Any(c => c == CaveCell.Empty);
            // var stone_count = GetNeighborCountWhere(x, y, c => c == CaveCell.Stone);
            newMap[y, x] = be_wall ? CaveCell.Wall : Map[y, x];
        }

        Map = newMap;
    }
}