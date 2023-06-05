using System.Diagnostics;
using System.Text;
using SixLabors.ImageSharp.Drawing.Processing;

namespace PCG.Maze.MazeShape;

public class Grid : IMazeMap<GridCell>
{
    public int Width { get; set; }
    public int Height { get; set; }

    public GridCell?[,] Cells { get; set; }

    public virtual IEnumerable<GridCell> GetAllCells() => Cells.OfType<GridCell>().ToList();

    public Grid(int width, int height)
    {
        Width = width;
        Height = height;

        Debug.Assert(Width >= 2 && Height >= 2);

        InitCells();

        // 设置 Cell 的邻居方向
        SetAllCellsNeighbor();
    }

    public Grid(MaskGrid maskGrid)
    {
        Width = maskGrid.Width;
        Height = maskGrid.Height;

        Debug.Assert(Width >= 2 && Height >= 2);

        Cells = new GridCell[Height, Width];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            if (maskGrid[y, x] != MaskGrid.InaccesableValue)
                Cells[y, x] = new GridCell(x, y);

        // 设置 Cell 的邻居方向
        SetAllCellsNeighbor();
    }

    protected virtual void InitCells()
    {
        Cells = new GridCell[Height, Width];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            Cells[y, x] = InitCell(x, y);
    }

    public virtual GridCell InitCell(int x, int y) => new GridCell(x, y);

    protected virtual void SetAllCellsNeighbor()
    {
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var cur_cell = Cells[y, x];
            if (cur_cell is null)
                continue;
            if (x + 1 < Width)
            {
                var right_cell = Cells[y, x + 1];
                if (right_cell != null)
                {
                    cur_cell.Right = right_cell;
                    right_cell.Left = cur_cell;
                }
            }

            if (y + 1 < Height)
            {
                var down_cell = Cells[y + 1, x];
                if (down_cell != null)
                {
                    cur_cell.Down = down_cell;
                    down_cell.Up = cur_cell;
                }
            }
        }
    }

    public string CellToString(GridCell cell) => "    ";

    public void Print()
    {
        var sb = new StringBuilder();
        const string emptyHorizontalString = "    ";
        const string wallHorizontalString = "----";
        sb.AppendLine(string.Join(wallHorizontalString, Enumerable.Repeat("+", Width + 1)));
        for (var y = 0; y < Height; y++)
        {
            // Check Right link
            sb.Append('|');
            for (var x = 0; x < Width; x++)
            {
                var cell = Cells[y, x];
                sb.Append(CellToString(cell));
                var is_linked_right = cell.HasRight && cell.IsLinked(cell.Right);
                sb.Append(is_linked_right ? " " : "|");
            }

            sb.AppendLine();

            // Check Bottom Link
            sb.Append('+');
            for (var x = 0; x < Width; x++)
            {
                var cell = Cells[y, x];
                var is_linked_bottom = cell.HasDown && cell.IsLinked(cell.Down);
                sb.Append(is_linked_bottom ? emptyHorizontalString : wallHorizontalString);
                sb.Append('+');
            }

            sb.AppendLine();
        }

        Console.WriteLine(sb.ToString());
    }

    protected const int CellWidth = 16;
    protected const int CellHeight = 16;
    protected const int Inset = 2;
    protected const int InnerCellWidth = CellWidth - 2 * Inset;
    protected const int InnerCellHeight = CellHeight - 2 * Inset;
    protected static SizeF cellSize = new(CellWidth, CellHeight);
    protected static SizeF innerCellSize = new(InnerCellWidth, InnerCellHeight);
    protected readonly Color lineColor = Color.Black;
    protected readonly Color baseColor = Color.White;
    protected readonly int lineThickness = 1;


    protected virtual (PointF lt, PointF lb, PointF rb, PointF rt) GetCellPoints(int x, int y)
    {
        var lt = new PointF(x * CellWidth, y * CellHeight);
        var lb = new PointF(x * CellWidth, (y + 1) * CellHeight);
        var rb = new PointF((x + 1) * CellWidth, (y + 1) * CellHeight);
        var rt = new PointF((x + 1) * CellWidth, y * CellHeight);
        return (lt, lb, rb, rt);
    }

    public virtual Image<Rgba32> DrawImage(Func<GridCell, Rgba32>? cellColorGetter = null)
    {
        Rgba32 DefaultCellColorGetter(GridCell cell) =>
            new Rgba32((float)(cell.X + 1) / Width, (float)(cell.Y + 1) / Height, 0f, 1f);

        // cellColorGetter ??= DefaultCellColorGetter;

        var (image_width, image_height) = GetImageSize();
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
                    var (lt, lb, rb, rt) = GetCellPoints(x, y);
                    var rgba32 = cellColorGetter(cell);
                    // var cell_color = Color.Blue;
                    var cell_color = new Color(rgba32);
                    // 先填充格子，然后填充格子 左边 和 上边 的线，保证线能够画在格子上面
                    // 因为格子的遍历顺序就是 从左往右 从上往下，之后这个格子上面的线和左边的 线都不会再次被格子本身覆盖掉
                    ctx.Fill(cell_color, new RectangleF(lt, cellSize));
                }
            }

            void DrawAllCellsLines()
            {
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cells[y, x];
                    if (cell is null) continue;

                    var (lt, lb, rb, rt) = GetCellPoints(x, y);

                    if (!cell.HasLinkUp)
                        ctx.DrawLines(line_color, lineThickness, lt, rt);
                    if (!cell.HasLinkLeft)
                        ctx.DrawLines(line_color, lineThickness, lt, lb);
                    if (!cell.HasLinkDown)
                        ctx.DrawLines(line_color, lineThickness, lb, rb);
                    if (!cell.HasLinkRight)
                        ctx.DrawLines(line_color, lineThickness, rt, rb);
                }
            }

            // 从上往下从左往右，对每个网格只绘制它的左侧和上侧，最后再加上顶线和底线
            void DrawCellLinesInFullGrid()
            {
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cells[y, x];
                    Debug.Assert(cell != null);

                    var (lt, lb, rb, rt) = GetCellPoints(x, y);

                    if (!cell.HasLinkUp)
                        ctx.DrawLines(line_color, lineThickness, lt, rt);
                    if (!cell.HasLinkLeft)
                        ctx.DrawLines(line_color, lineThickness, lt, lb);
                }

                // Draw Right Line
                ctx.DrawLines(line_color, lineThickness, new PointF(image_width, 0),
                    new PointF(image_width, image_height));
                // Draw Bottom Line
                ctx.DrawLines(line_color, lineThickness, new PointF(0, image_height),
                    new PointF(image_width, image_height));
            }

            FillCellsColor();
            DrawAllCellsLines();
            // DrawCellLinesInFullGrid();
        });

        return image;
    }

    protected virtual (int image_width, int image_height) GetImageSize()
    {
        var image_width = Width * CellWidth;
        var image_height = Height * CellHeight;
        return (image_width, image_height);
    }

    protected struct InsetCellPoints
    {
        public int x0, x1, x2, x3;
        public int y0, y1, y2, y3;
    }


    protected InsetCellPoints GetInsetCoordinate(int x, int y)
    {
        var i = new InsetCellPoints();

        i.x0 = x * CellWidth;
        i.x3 = (x + 1) * CellWidth;
        i.x1 = i.x0 + Inset;
        i.x2 = i.x3 - Inset;

        i.y0 = y * CellHeight;
        i.y3 = (y + 1) * CellHeight;
        i.y1 = i.y0 + Inset;
        i.y2 = i.y3 - Inset;

        return i;
    }

    public virtual void DrawImageWithInset(Image<Rgba32> image, Func<GridCell, Rgba32>? cellColorGetter = null)
    {
        image.Mutate(ctx =>

        {
            void FillCellsColor()
            {
                if (cellColorGetter is null) return;
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cells[y, x];
                    if (cell is null) continue;
                    var lt = new PointF(x * CellWidth + Inset, y * CellHeight + Inset);
                    var rgba32 = cellColorGetter(cell);
                    // var cell_color = Color.Blue;
                    var cell_color = new Color(rgba32);
                    // 先填充格子，然后填充格子 左边 和 上边 的线，保证线能够画在格子上面
                    // 因为格子的遍历顺序就是 从左往右 从上往下，之后这个格子上面的线和左边的 线都不会再次被格子本身覆盖掉
                    ctx.Fill(cell_color, new RectangleF(lt, innerCellSize));

                    var i = GetInsetCoordinate(x, y);
                    if (cell.HasLinkUp)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x1, i.y0, InnerCellWidth, Inset));
                    }

                    if (cell.HasLinkDown)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x1, i.y2, InnerCellWidth, Inset));
                    }

                    if (cell.HasLinkLeft)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x0, i.y1, Inset, InnerCellHeight));
                    }

                    if (cell.HasLinkRight)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x2, i.y1, Inset, InnerCellHeight));
                    }
                }
            }

            void DrawLine(int x0, int y0, int x1, int y1) =>
                ctx.DrawLines(lineColor, lineThickness, new PointF(x0, y0), new PointF(x1, y1));

            void DrawAllCellsLines()
            {
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cells[y, x];
                    if (cell is null) continue;

                    var i = GetInsetCoordinate(x, y);

                    if (cell.HasLinkUp)
                    {
                        DrawLine(i.x1, i.y1, i.x1, i.y0);
                        DrawLine(i.x2, i.y1, i.x2, i.y0);
                    }
                    else
                    {
                        DrawLine(i.x1, i.y1, i.x2, i.y1);
                    }

                    if (cell.HasLinkDown)
                    {
                        DrawLine(i.x1, i.y2, i.x1, i.y3);
                        DrawLine(i.x2, i.y2, i.x2, i.y3);
                    }
                    else
                    {
                        DrawLine(i.x1, i.y2, i.x2, i.y2);
                    }

                    if (cell.HasLinkLeft)
                    {
                        DrawLine(i.x0, i.y1, i.x1, i.y1);
                        DrawLine(i.x0, i.y2, i.x1, i.y2);
                    }
                    else
                    {
                        DrawLine(i.x1, i.y1, i.x1, i.y2);
                    }

                    if (cell.HasLinkRight)
                    {
                        DrawLine(i.x3, i.y1, i.x2, i.y1);
                        DrawLine(i.x3, i.y2, i.x2, i.y2);
                    }
                    else
                    {
                        DrawLine(i.x2, i.y1, i.x2, i.y2);
                    }
                }
            }

            FillCellsColor();
            DrawAllCellsLines();
        });
    }

    public virtual Image<Rgba32> DrawImageWithInset(Func<GridCell, Rgba32>? cellColorGetter = null)
    {
        var image = InitImageToDraw();

        DrawImageWithInset(image);

        return image;
    }

    protected Image<Rgba32> InitImageToDraw()
    {
        var image_width = Width * CellWidth;
        var image_height = Height * CellHeight;
        var image = new Image<Rgba32>(image_width, image_height);
        // 先填充底色
        image.Mutate(ctx =>
            ctx.Fill(baseColor, new RectangleF(0, 0, image.Width, image.Height)));
        return image;
    }

    public void RemoveCell(GridCell cell)
    {
        Debug.Assert(Cells[cell.Y, cell.X] == cell);
        Cells[cell.Y, cell.X] = null;
        if (cell.Left != null) cell.Left.Right = null;
        if (cell.Right != null) cell.Right.Left = null;
        if (cell.Up != null) cell.Up.Down = null;
        if (cell.Down != null) cell.Down.Up = null;

        foreach (var grid_cell in cell.GetLinks())
        {
            cell.UnLink(grid_cell, true);
        }
    }
}