namespace PCG.Maze.MazeShape;

public interface IMazeMap
{
    public List<Cell> GetAllCells();
    public void Print();
    public Image<Rgba32> DrawImage(Func<Cell, Rgba32>? cellColorGetter = null);
}