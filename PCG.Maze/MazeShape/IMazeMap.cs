namespace PCG.Maze.MazeShape;

public interface IMazeMap<TCell> where TCell: CellBase
{
    public IEnumerable<TCell> GetAllCells();
    public void Print();
    public Image<Rgba32> DrawImage(Func<TCell, Rgba32>? cellColorGetter = null);
    public void RemoveCell(TCell cell);
}

public interface IMazeMap
{
    public IEnumerable<CellBase> GetAllCells();
    public void Print();
    public Image<Rgba32> DrawImage(Func<CellBase, Rgba32>? cellColorGetter = null);
}