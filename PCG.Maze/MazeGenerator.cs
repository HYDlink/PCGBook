using System.Diagnostics;

namespace PCG.Maze;

public class MazeGenerator
{
    private Grid grid;

    public MazeGenerator(Grid grid)
    {
        this.grid = grid;
    }

    public void BinaryTreeLink()
    {
        var seed = new Random().Next();
        Console.WriteLine($"Binary Tree Link, Random seed: {seed}");
        var random = new Random(seed);
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
        }
    }

    public void SidewinderLink()
    {
        var seed = new Random().Next();
        Console.WriteLine($"Sidewinder Link, Random seed: {seed}");
        var random = new Random(seed);
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
            }
        }
    }

    /// <summary>
    /// 随机选择一个点开始，然后随机选择其中的邻居，<br/>
    ///     如果这个邻居没有被访问，那么和这个邻居连接起来，否则什么都不做<br/>
    ///     从这个新邻居开始，继续随机访问它的邻居
    /// </summary>
    public void AldousBroderLink()
    {
        var seed = new Random().Next();
        Console.WriteLine($"Aldous-Broder Link, Random seed: {seed}");
        var random = new Random(seed);

        var unvisited = grid.Cells.Cast<Cell>().ToList();
        Shuffle(unvisited, random);

        var last = unvisited.Last();
        unvisited.Remove(last);

        while (unvisited.Any())
        {
            var neighbor = Grid.GetRandomNeighbor(last, random);
            if (unvisited.Contains(neighbor))
            {
                last.Link(neighbor, true);
                unvisited.Remove(neighbor);
            }

            last = neighbor;
        }
    }

    /// <summary>
    /// 首先选取一个点作为已经访问的点；
    /// 随机选取未访问的点，开始随机游走，直到遇到访问过的点，并将整个路径连接起来，全部标记为访问过；
    /// 游走的过程中如果形成了环，那就消除这个环路径；
    /// 如此循环，直到所有点都被访问过
    /// </summary>
    public void WilsonLink()
    {
        var seed = new Random().Next();
        Console.WriteLine($"Wilson Link, Random seed: {seed}");
        // var random = new Random(seed);
        var random = new Random(1251779763);

        var unvisited = grid.Cells.Cast<Cell>().ToList();
        Shuffle(unvisited, random);
        var start_visit = unvisited.Last();
        unvisited.Remove(start_visit);

        while (unvisited.Any())
        {
            var last = unvisited.Last();
            var path = new List<Cell> { last };
            while (true)
            {
                var neighbor = Grid.GetRandomNeighbor(last, random);

                // 移除环状路径
                var index = path.IndexOf(neighbor);
                if (index != -1)
                {
                    path.RemoveRange(index, path.Count - index);
                }

                path.Add(neighbor);

                var is_connect_to_visited = !unvisited.Contains(neighbor);
                if (is_connect_to_visited)
                    break;
                last = neighbor;
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                path[i].Link(path[i + 1], true);
                unvisited.Remove(path[i]);
            }

            unvisited.Remove(path.Last());
        }
    }

    public void BackTrackLink()
    {
        var seed = new Random().Next();
        Console.WriteLine($"BackTrack Link, Random seed: {seed}");
        var random = new Random(seed);

        var unvisited = grid.Cells.Cast<Cell>().ToList();

        void DFS(Cell cell)
        {
            unvisited.Remove(cell);
            var neighbors = cell.GetNeighbors().ToList();
            Shuffle(neighbors, random);
            foreach (var neighbor in neighbors)
            {
                if (unvisited.Contains(neighbor))
                {
                    cell.Link(neighbor, true);
                    DFS(neighbor);
                }
            }
        }

        DFS(grid.Cells[0, 0]);
    }

    public static void Shuffle<T>(IList<T> list, Random rng)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}