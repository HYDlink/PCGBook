namespace PCG.Maze.MazeShape;

public record Map2D(int Width, int Height)
{
    protected int[,] values = new int[Height, Width];

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
}