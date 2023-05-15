namespace PCG.Maze.MazeShape;

public interface IMazeMap<out TCell> where TCell: CellBase
{
    public IEnumerable<TCell> GetAllCells();
    public void Print();
    public Image<Rgba32> DrawImage(Func<TCell, Rgba32>? cellColorGetter = null);
}