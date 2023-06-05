using System.Diagnostics;

namespace PCG.Maze.MazeShape.Advanced;

public class Grid3D : IMazeMap<Grid3DCell>
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }

    public Grid3DCell?[,,] Cells { get; set; }
    public virtual IEnumerable<Grid3DCell> GetAllCells() => Cells.OfType<Grid3DCell>().ToList();

    public Grid3D(int width, int height, int length)
    {
        Width = width;
        Height = height;
        Length = length;

        Debug.Assert(Width >= 2 && Height >= 2);

        InitCells();

        // 设置 Cell 的邻居方向
        SetAllCellsNeighbor();
    }

    protected virtual void InitCells()
    {
        Cells = new Grid3DCell[Length, Height, Width];
        for (var z = 0; z < Length; z++)
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            Cells[z, y, x] = InitCell(x, y, z);
    }


    public virtual Grid3DCell InitCell(int x, int y, int z) => new Grid3DCell(x, y, z);

    protected virtual void SetAllCellsNeighbor()
    {
        for (var z = 0; z < Length; z++)
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var cur_cell = Cells[z, y, x];
            if (cur_cell is null)
                continue;
            if (x + 1 < Width)
            {
                var right_cell = Cells[z, y, x + 1];
                if (right_cell != null)
                {
                    cur_cell.Right = right_cell;
                    right_cell.Left = cur_cell;
                }
            }

            if (y + 1 < Height)
            {
                var down_cell = Cells[z, y + 1, x];
                if (down_cell != null)
                {
                    cur_cell.Down = down_cell;
                    down_cell.Up = cur_cell;
                }
            }

            if (z + 1 < Length)
            {
                var forward_cell = Cells[z + 1, y, x];
                if (forward_cell != null)
                {
                    cur_cell.Forward = forward_cell;
                    forward_cell.Backward = cur_cell;
                }
            }
        }
    }

    public Grid3DCell GetCell(int x, int y, int z) => Cells[z, y, x];

    public void Print()
    {
        throw new NotImplementedException();
    }

    public Image<Rgba32> DrawImage(Func<Grid3DCell, Rgba32>? cellColorGetter = null)
    {
        throw new NotImplementedException();
    }

    public void RemoveCell(Grid3DCell cell)
    {
        throw new NotImplementedException();
    }
}