using Raylib_cs;

namespace PCG.Maze.MazeShape.Advanced;

public class MobiusPlane : CylinderGrid
{
    public MobiusPlane(int width, int height) : base(width * 2, height)
    {
    }

    public MobiusPlane(MaskGrid maskGrid) : base(maskGrid)
    {
    }

    protected override (int image_width, int image_height) GetImageSize() 
        => (Width * CellWidth / 2, Height * 2 *CellWidth);

    protected override (PointF lt, PointF lb, PointF rb, PointF rt) GetCellPoints(int x, int y)
    {
        if (x + 1 > Width / 2)
            return base.GetCellPoints(x - Width / 2, y + Height);
        else
            return base.GetCellPoints(x, y);
    }
}

public class MobiusPlayer : RaylibPlayer
{
    protected void InitMesh()
    {
        var mesh = new Mesh();
        
    }
}