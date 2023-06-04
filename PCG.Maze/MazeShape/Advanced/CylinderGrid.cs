namespace PCG.Maze.MazeShape.Advanced;

public class CylinderGrid : Grid
{
    public CylinderGrid(int width, int height) : base(width, height)
    {
    }

    public CylinderGrid(MaskGrid maskGrid) : base(maskGrid)
    {
    }
}