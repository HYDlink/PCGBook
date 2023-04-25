// ReSharper disable MemberCanBePrivate.Global

using System.Collections;
using System.Diagnostics;
using PCG.CellularAutomata;

public static class CaveCell
{
    public const int Empty = 0;
    public const int Stone = 1;
    public const int Wall = 2;
    public const int ToDig = 3;
}

public static class SpecialCell
{
    public const int Any = -1;
    public const int Keep = -2;
}

public class CaveCA
{

    public double ToWallRatio { get; set; } = 0.4;
    public int Width { get; set; }
    public int Height { get; set; }
    public double CavePercent { get; set; }
    public int ExecuteTimes { get; set; }
    public int[,] Map { get; set; }

    private Random Random { get; set; } = new Random();
    public int Seed { get; set; }

    public CaveCA(int width, int height, double cavePercent, int executeTimes = 2)
    {
        Width = width;
        Height = height;
        CavePercent = cavePercent;
        ExecuteTimes = executeTimes;
        Map = new int[Height, Width];
        RoomMap = new int[Height, Width];
    }

    public int GetCell(int x, int y) => Map[y, x];

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

        Map = new int[Height, Width];

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
            Map[y, x] = Random.NextDouble() < CavePercent
                ? CaveCell.Stone
                : CaveCell.Empty;
    }

    public static readonly CARule rule =
        new CellCountRule(SpecialCell.Any, CaveCell.Stone, CaveCell.Stone, NeighborType.Square, 5);

    private CARule[] CaRules = { rule };

    public void Automation()
        => AutomationByRule(CaRules);
    public void AutomationFormal()
    {
        var newMap = new int[Height, Width];

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var all_neighbors = GetAllNeighborsInSquare(x, y);
            // ReSharper disable PossibleMultipleEnumeration
            var be_stone = (all_neighbors.Count(IsStone) / (double)all_neighbors.Count()) > ToWallRatio;
            // var stone_count = GetNeighborCountWhere(x, y, c => c == CaveCell.Stone);
            newMap[y, x] = be_stone ? CaveCell.Stone : CaveCell.Empty;
        }

        Map = newMap;
    }

    public void AutomationByRule(IEnumerable<CARule> rules)
    {
        
        var newMap = new int[Height, Width];

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var results = rules
                .Select(r => (r.GetNewValue(this, x, y, out var result), result)).ToList();
            var cell = results
                .FirstOrDefault(t => t.Item1).result;
            newMap[y, x] = cell;
        }

        Map = newMap;
    }

    private bool IsStone(int c) => c is CaveCell.Stone or CaveCell.Wall;

    public void DrawEdge()
    {
        var newMap = new int[Height, Width];

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
    public List<Room> Rooms { get; set; }

    public void FillRoom()
    {
        RoomMap = new int[Height, Width];
        Rooms = new List<Room>();
        var curRoomId = 1;
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            if (!CanBeFilled(x, y)) continue;
            var room = FloodFillRoomById(x, y, curRoomId);
            Rooms.Add(room);
            curRoomId++;
        }
    }

    /// <summary>
    /// 使用迭代器来延时执行每个 Connection 的Dig
    /// </summary>
    /// <returns></returns>
    public IEnumerable ConnectRooms()
    {
        if (Rooms == null) yield break;
        var connections = ConnectRooms(Rooms);
        // connections.ForEach(DigConnection);
        foreach (var connection in connections)
        {
            DigConnection(connection);
            yield return null;
        }
    }

    public bool CanBeFilled(int x, int y)
        => Map[y, x] == CaveCell.Empty && RoomMap[y, x] == 0;

    private Room FloodFillRoomById(int x, int y, int roomId)
    {
        var neighbors = new List<Pos>() { new(y, x) };
        var rooms = new List<Pos>() { new(y, x) };
        while (neighbors.Any())
        {
            neighbors.ForEach(tuple => RoomMap[tuple.Y, tuple.X] = roomId);
            var new_neighbors = neighbors
                .SelectMany(xy => GetAllNeighborsPosInCross(xy.X, xy.Y))
                .Where(xy => CanBeFilled(xy.X, xy.Y)).Distinct().ToList();
            neighbors = new_neighbors;
            rooms.AddRange(new_neighbors);
        }

        return new Room(roomId, rooms, GetEdgesInSpace(rooms));
    }

    public List<Pos> GetEdgesInSpace(List<Pos> space)
        => space.Where(p => GetAllNeighborsInCross(p.X, p.Y).Any(IsStone)).ToList();

    public record struct Pos(int Y, int X);

    public record Room(int Id, List<Pos> Cells, List<Pos> Edges);

    public record Connection(Room A, Room B, Pos EdgeA, Pos EdgeB);

    public List<Connection> ConnectRooms(List<Room> rooms)
    {
        var cons = new List<Connection>();
        for (int i = 0; i < rooms.Count; i++)
        for (int j = i + 1; j < rooms.Count; j++)
        {
            var a = rooms[i];
            var b = rooms[j];
            Debug.Assert(a.Edges.Count > 0 && b.Edges.Count > 0);
            var min_dist = int.MaxValue;
            var e_a = a.Edges[0];
            var e_b = b.Edges[0];
            foreach (var edge_a in a.Edges)
            {
                foreach (var edge_b in b.Edges)
                {
                    var dist_x = edge_a.X - edge_b.X;
                    var dist_y = edge_a.Y - edge_b.Y;
                    var square_dist = dist_x * dist_x + dist_y * dist_y;
                    if (square_dist < min_dist)
                    {
                        min_dist = square_dist;
                        e_a = edge_a;
                        e_b = edge_b;
                    }
                }
            }

            const int squareDistThreshold = 64;
            if (min_dist <= squareDistThreshold)
                cons.Add(new Connection(a, b, e_a, e_b));
        }

        return cons;
    }

    public void DigConnection(Connection connection)
    {
        var (y0, x0) = connection.EdgeA;
        var (y1, x1) = connection.EdgeB;

        // test dig point
        // Map[y0, x0] = CaveCell.ToDig;
        // Map[y1, x1] = CaveCell.ToDig;
        // return;

        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2; /* error value e_xy */

        for (;;)
        {
            /* loop */
            // Map[y0, x0] = CaveCell.ToDig;
            Map[y0, x0] = CaveCell.Empty;
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            } /* e_xy+e_x > 0 */

            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            } /* e_xy+e_y < 0 */
        }
    }

    #endregion

    #region Neighbors

    public IEnumerable<Pos> GetAllNeighborsPosInSquare(int x, int y)
    {
        if (x > 0)
        {
            var nx = x - 1;
            yield return new Pos(y, nx);
            if (y > 0)
                yield return new Pos(y - 1, nx);
            if (y < Height - 1)
                yield return new Pos(y + 1, nx);
        }

        if (y > 0)
            yield return new Pos(y - 1, x);
        if (y < Height - 1)
            yield return new Pos(y + 1, x);

        if (x < Width - 1)
        {
            var nx = x + 1;
            yield return new Pos(y, nx);
            if (y > 0)
                yield return new Pos(y - 1, nx);
            if (y < Height - 1)
                yield return new Pos(y + 1, nx);
        }
    }

    public IEnumerable<Pos> GetAllNeighborsPosInCross(int x, int y)
    {
        if (x > 0)
        {
            var nx = x - 1;
            yield return new Pos(y, nx);
        }

        if (y > 0)
            yield return new Pos(y - 1, x);
        if (y < Height - 1)
            yield return new Pos(y + 1, x);

        if (x < Width - 1)
        {
            var nx = x + 1;
            yield return new Pos(y, nx);
        }
    }

    public IEnumerable<int> GetAllNeighborsInCross(Pos pos)
        => GetAllNeighborsInCross(pos.X, pos.Y);

    public IEnumerable<int> GetAllNeighborsInCross(int x, int y)
        => GetAllNeighborsPosInCross(x, y).Select(pos => Map[pos.Y, pos.X]);

    public IEnumerable<int> GetAllNeighborsInSquare(int x, int y)
        => GetAllNeighborsPosInSquare(x, y).Select(pos => Map[pos.Y, pos.X]);

    public IEnumerable<int> GetAllNeighbors(NeighborType neighborType, int x, int y)
        => neighborType == NeighborType.Square ? GetAllNeighborsInSquare(x, y) : GetAllNeighborsInCross(x, y);

    public IEnumerable<Pos> GetAllNeighborsPos(NeighborType neighborType, int x, int y)
        => neighborType == NeighborType.Square
            ? GetAllNeighborsPosInSquare(x, y)
            : GetAllNeighborsPosInCross(x, y);

    #endregion
}