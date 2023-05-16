using PCG.Maze.MazeShape;

namespace PCG.Maze.ValueMap;

public class DistanceMap<TCell> where TCell : CellBase
{
    public IMazeMap<TCell> Grid { get; private set; }
    public int MaxDist { get; private set; }

    private Dictionary<CellBase, int> values = new();

    public int this[TCell cell]
    {
        get => values[cell];
        set => values[cell] = value;
    }

    public DistanceMap(IMazeMap<TCell> grid, TCell? startCell)
    {
        ArgumentNullException.ThrowIfNull(startCell);
        Grid = grid;
        foreach (var cell in grid.GetAllCells())
        {
            this[cell] = Int32.MaxValue;
        }

        this[startCell] = 0;

        void Dfs(TCell cell)
        {
            foreach (var neighbor in cell.GetLinks().OfType<TCell>())
            {
                var new_dist = this[cell] + 1;
                var formal_dist = this[neighbor];
                if (new_dist < formal_dist)
                {
                    this[neighbor] = new_dist;
                    Dfs(neighbor);
                }
            }
        }

        Dfs(startCell);
        MaxDist = values.Values.Max();
    }

    private DistanceMap(IMazeMap<TCell> grid)
    {
        Grid = grid;
    }

    /// <summary>
    /// 返回一个距离图，除了从起点到达 <paramref name="endCell"/> 的路径保留了路径计算，所有其他的路径都被染上了最大值距离的距离图
    /// </summary>
    /// <param name="endCell"></param>
    /// <returns></returns>
    public DistanceMap<TCell> GetPathMap(TCell endCell)
    {
        var path_map = new DistanceMap<TCell>(Grid) { MaxDist = MaxDist };

        foreach (var cell in Grid.GetAllCells())
        {
            path_map[cell] = MaxDist;
        }

        path_map[endCell] = this[endCell];

        var cur_cell = endCell;
        while (this[cur_cell] != 0)
        {
            var prev_path = cur_cell.GetLinks().OfType<TCell>().FirstOrDefault(c => this[c] < this[cur_cell]);
            if (prev_path is null)
                break;
            path_map[prev_path] = this[prev_path];
            cur_cell = prev_path;
        }

        return path_map;
    }

    public Func<TCell, Rgba32> GetCellColorByDistanceValue(bool newColorWithMaxDist = false)
    {
        Rgba32 GetColor(TCell cell)
        {
            float dist_percent = (float)this[cell] / MaxDist;
            return new Rgba32(1f, 0, 1f, dist_percent);
        }

        Rgba32 GetColorSpecial(TCell cell)
        {
            if (this[cell] == MaxDist)
                return new Rgba32(0, 0, 1f, 1f);
            return GetColor(cell);
        }

        return newColorWithMaxDist ? GetColorSpecial : GetColor;
    }
}