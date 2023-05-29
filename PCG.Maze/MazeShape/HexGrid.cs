using System.Diagnostics;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using Path = SixLabors.ImageSharp.Drawing.Path;

namespace PCG.Maze.MazeShape;

public class HexGrid : IMazeMap<HexCell>
{
    public int Width { get; set; }
    public int Height { get; set; }

    public HexCell?[,] Cells { get; set; }

    public virtual IEnumerable<HexCell> GetAllCells() => Cells.OfType<HexCell>().ToList();

    public HexGrid(int width, int height)
    {
        Width = width;
        Height = height;

        // Debug.Assert(Width >= 2 && Height >= 2);

        InitCells();

        // 设置 Cell 的邻居方向
        SetAllCellsNeighbor();
    }

    protected virtual void InitCells()
    {
        Cells = new HexCell[Height, Width];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            Cells[y, x] = InitCell(x, y);
    }

    public virtual HexCell InitCell(int x, int y) => new HexCell(x, y);

    protected virtual void SetAllCellsNeighbor()
    {
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var cur_cell = Cells[y, x];
            if (x + 1 < Width)
            {
                var right = Cells[y, x + 1];
                cur_cell.Right = right;
                right.Left = cur_cell;
            }

            if (y + 1 < Height)
            {
                var is_right_offset_line = y % 2 == 1;
                if (is_right_offset_line || x > 0)
                {
                    var lb = Cells[y + 1, is_right_offset_line ? x : x - 1];
                    cur_cell.LB = lb;
                    lb.RT = cur_cell;
                }
                if (!is_right_offset_line || x + 1 < Width)
                {
                    var rb = Cells[y + 1, is_right_offset_line ? x + 1 : x];
                    cur_cell.RB = rb;
                    rb.LT = cur_cell;
                }
            }
        }
    }

    public void Print()
    {
        throw new NotImplementedException();
    }

    protected const int HalfCellWidth = 14;
    protected const int QuarterCellHeight = 8;
    protected const int HalfCellHeight = QuarterCellHeight * 2;
    protected const int CellHeightPadding = QuarterCellHeight * 3;
    protected const int CellWidth = HalfCellWidth * 2;
    protected const int CellHeight = HalfCellHeight * 2;
    protected readonly Color lineColor = Color.Black;
    protected readonly Color baseColor = Color.White;
    protected readonly int lineThickness = 1;


    protected readonly PointF CellLt = new PointF(0, QuarterCellHeight);
    protected readonly PointF CellLb = new PointF(0, 3 * QuarterCellHeight);
    protected readonly PointF CellRb = new PointF(CellWidth, 3 * QuarterCellHeight);
    protected readonly PointF CellRt = new PointF(CellWidth, QuarterCellHeight);
    protected readonly PointF CellTop = new PointF(HalfCellWidth, 0);
    protected readonly PointF CellBottom = new PointF(HalfCellWidth, CellHeight);

    protected PointF GetCellOffset(int x, int y)
        => new(x * CellWidth + (((y % 2) == 0) ? 0 : HalfCellWidth), y * CellHeightPadding);

    protected (PointF lt, PointF lb, PointF rb, PointF rt, PointF top, PointF bottom) GetCellPoints(int x, int y)
    {
        var offset = GetCellOffset(x, y);
        return (CellLt + offset,
            CellLb + offset,
            CellRb + offset,
            CellRt + offset,
            CellTop + offset,
            CellBottom + offset);
    }

    public Image<Rgba32> DrawImage(Func<HexCell, Rgba32>? cellColorGetter = null)
    {
        var image_width = Width * CellWidth + HalfCellWidth;
        var image_height = Height * CellHeightPadding + QuarterCellHeight;
        var image = new Image<Rgba32>(image_width, image_height);
        image.Mutate(ctx =>
        {
            // 先填充底色
            var base_color = Color.White;
            ctx.Fill(base_color, new RectangleF(0, 0, image_width, image_height));

            var line_color = Color.Black;

            void FillCellsColor()
            {
                if (cellColorGetter is null) return;
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cells[y, x];
                    if (cell is null) continue;
                    var rgba32 = cellColorGetter(cell);
                    var cell_color = new Color(rgba32);

                    var (lt, lb, rb, rt, top, bottom) = GetCellPoints(x, y);
                    ctx.Fill(cell_color, new Path(new LinearLineSegment[]
                        { new(lt, top), new(top, rt), new(rt, rb), new(rb, bottom), new(bottom, lb), new(lb, lt) }));
                }
            }

            void DrawAllCellsLines()
            {
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cells[y, x];
                    if (cell is null) continue;

                    var (lt, lb, rb, rt, top, bottom) = GetCellPoints(x, y);
                    if (!cell.HasLinkedLT) ctx.DrawLines(line_color, lineThickness, lt, top);
                    if (!cell.HasLinkedRT) ctx.DrawLines(line_color, lineThickness, top, rt);
                    if (!cell.HasLinkedRight) ctx.DrawLines(line_color, lineThickness, rt, rb);
                    if (!cell.HasLinkedRB) ctx.DrawLines(line_color, lineThickness, rb, bottom);
                    if (!cell.HasLinkedLB) ctx.DrawLines(line_color, lineThickness, bottom, lb);
                    if (!cell.HasLinkedLeft) ctx.DrawLines(line_color, lineThickness, lb, lt);
                }
            }

            FillCellsColor();
            DrawAllCellsLines();
        });
        return image;
    }

    public void RemoveCell(HexCell cell)
    {
        throw new NotImplementedException();
    }
}