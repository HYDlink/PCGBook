using System.Diagnostics;
using System.Text;
using SixLabors.ImageSharp.Drawing.Processing;

namespace PCG.Maze.MazeShape;

public class Grid : IMazeMap<GridCell>
{
    public int Width { get; set; }
    public int Height { get; set; }

    public GridCell?[,] Cells { get; set; }

    public IEnumerable<GridCell> GetAllCells() => Cells.OfType<GridCell>().ToList();

    public Grid(int width, int height)
    {
        Width = width;
        Height = height;

        Debug.Assert(Width >= 2 && Height >= 2);

        Cells = new GridCell[Height, Width];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            Cells[y, x] = new GridCell(x, y);

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

    private void SetAllCellsNeighbor()
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

    public Image<Rgba32> DrawImage(Func<GridCell, Rgba32>? cellColorGetter = null)
    {
        Rgba32 defaultCellColorGetter(GridCell cell) =>
            new Rgba32((float)(cell.X + 1) / Width, (float)(cell.Y + 1) / Height, 0f, 1f);

        // cellColorGetter ??= defaultCellColorGetter;

        const int cellWidth = 8;
        const int cellHeight = 8;
        var cell_size = new SizeF(cellWidth, cellHeight);
        var image_width = Width * cellWidth;
        var image_height = Height * cellHeight;
        var image = new Image<Rgba32>(image_width, image_height);
        var line_thickness = 2;
        image.Mutate(ctx =>
        {
            // 先填充底色
            var base_color = Color.White;
            ctx.Fill(base_color, new RectangleF(0, 0, image_width, image_height));

            var line_color = Color.Black;

            void FillCellsColor()
            {
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    if (cellColorGetter is null) continue;
                    var cell = Cells[y, x];
                    if (cell is null) continue;
                    var lt = new PointF(x * cellWidth, y * cellHeight);
                    var rgba32 = cellColorGetter(cell);
                    // var cell_color = Color.Blue;
                    var cell_color = new Color(rgba32);
                    // 先填充格子，然后填充格子 左边 和 上边 的线，保证线能够画在格子上面
                    // 因为格子的遍历顺序就是 从左往右 从上往下，之后这个格子上面的线和左边的 线都不会再次被格子本身覆盖掉
                    ctx.Fill(cell_color, new RectangleF(lt, cell_size));
                }
            }

            // DrawGridLines
            // 之前采用的是，从上往下从左往右，对每个网格只绘制它的左侧和上侧，最后再加上顶线和底线

            void DrawAllCellsLines()
            {
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cells[y, x];
                    if (cell is null) continue;

                    var lt = new PointF(x * cellWidth, y * cellHeight);
                    var lb = new PointF(x * cellWidth, (y + 1) * cellHeight);
                    var rb = new PointF((x + 1) * cellWidth, (y + 1) * cellHeight);
                    var rt = new PointF((x + 1) * cellWidth, y * cellHeight);

                    if (!cell.HasLinkUp)
                        ctx.DrawLines(line_color, line_thickness, lt, rt);
                    if (!cell.HasLinkLeft)
                        ctx.DrawLines(line_color, line_thickness, lt, lb);
                    if (!cell.HasLinkDown)
                        ctx.DrawLines(line_color, line_thickness, lb, rb);
                    if (!cell.HasLinkRight)
                        ctx.DrawLines(line_color, line_thickness, rt, rb);
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

                    var lt = new PointF(x * cellWidth, y * cellHeight);
                    var lb = new PointF(x * cellWidth, (y + 1) * cellHeight);
                    var rb = new PointF((x + 1) * cellWidth, (y + 1) * cellHeight);
                    var rt = new PointF((x + 1) * cellWidth, y * cellHeight);

                    if (!cell.HasLinkUp)
                        ctx.DrawLines(line_color, line_thickness, lt, rt);
                    if (!cell.HasLinkLeft)
                        ctx.DrawLines(line_color, line_thickness, lt, lb);
                    if (!cell.HasLinkDown)
                        ctx.DrawLines(line_color, line_thickness, lb, rb);
                    if (!cell.HasLinkRight)
                        ctx.DrawLines(line_color, line_thickness, rt, rb);
                }

                // Top and Bottom Line
                ctx.DrawLines(line_color, line_thickness, new PointF(0, 0), new PointF(image_width, 0));
                ctx.DrawLines(line_color, line_thickness, new PointF(0, image_height),
                    new PointF(image_width, image_height));
            }

            FillCellsColor();
            DrawAllCellsLines();
        });

        return image;
        // Utilities.SaveImage(image, "Maze");
        // image.Dispose();
    }
}