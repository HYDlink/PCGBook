// ReSharper disable MemberCanBePrivate.Global

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
        Map = new CaveCell[Height, Width];
        RoomMap = new int[Height, Width];
    }

    #region Execution

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

    public void Automation()
    {
        var newMap = new CaveCell[Height, Width];

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var all_neighbors = GetAllNeighbors(x, y);
            // ReSharper disable PossibleMultipleEnumeration
            var be_stone = (all_neighbors.Count(IsStone) / (double)all_neighbors.Count()) > ToWallRatio;
            // var stone_count = GetNeighborCountWhere(x, y, c => c == CaveCell.Stone);
            newMap[y, x] = be_stone ? CaveCell.Stone : CaveCell.Empty;
        }

        Map = newMap;
    }

    private bool IsStone(CaveCell c) => c is CaveCell.Stone or CaveCell.Wall;

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

    #endregion

    #region Rooms

    public int[,] RoomMap { get; set; }

    public void FillRoom()
    {
        RoomMap = new int[Height, Width];
        var curRoomId = 1;
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            if (!CanBeFilled(x, y)) continue;
            FloodFillRoomById(x, y, curRoomId);
            curRoomId++;
        }
    }

    public bool CanBeFilled(int x, int y)
        => Map[y, x] == CaveCell.Empty && RoomMap[y, x] == 0;

    private void FloodFillRoomById(int x, int y, int roomId)
    {
        var neighbors = new List<(int Y, int X)>() { (y, x) };
        while (neighbors.Any())
        {
            neighbors.ForEach(tuple => RoomMap[tuple.Y, tuple.X] = roomId);
            var new_neighbors = neighbors
                .SelectMany(xy => GetAllNeighborsPosInCross(xy.X, xy.Y))
                .Where(xy => CanBeFilled(xy.X, xy.Y)).Distinct().ToList();
            neighbors = new_neighbors;
        }
    }

    #endregion

    #region Neighbors

    public IEnumerable<(int Y, int X)> GetAllNeighborsPos(int x, int y)
    {
        if (x > 0)
        {
            var nx = x - 1;
            yield return (y, nx);
            if (y > 0)
                yield return (y - 1, nx);
            if (y < Height - 1)
                yield return (y + 1, nx);
        }

        if (y > 0)
            yield return (y - 1, x);
        if (y < Height - 1)
            yield return (y + 1, x);

        if (x < Width - 1)
        {
            var nx = x + 1;
            yield return (y, nx);
            if (y > 0)
                yield return (y - 1, nx);
            if (y < Height - 1)
                yield return (y + 1, nx);
        }
    }

    public IEnumerable<(int Y, int X)> GetAllNeighborsPosInCross(int x, int y)
    {
        if (x > 0)
        {
            var nx = x - 1;
            yield return (y, nx);
        }

        if (y > 0)
            yield return (y - 1, x);
        if (y < Height - 1)
            yield return (y + 1, x);

        if (x < Width - 1)
        {
            var nx = x + 1;
            yield return (y, nx);
        }
    }

    public IEnumerable<CaveCell> GetAllNeighborsInCross(int x, int y)
        => GetAllNeighborsPosInCross(x, y).Select(pos => Map[pos.Y, pos.X]);

    public IEnumerable<CaveCell> GetAllNeighbors(int x, int y)
        => GetAllNeighborsPos(x, y).Select(pos => Map[pos.Y, pos.X]);

    #endregion
}