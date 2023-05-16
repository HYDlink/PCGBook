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
        Rgba32 DefaultCellColorGetter(GridCell cell) =>
            new Rgba32((float)(cell.X + 1) / Width, (float)(cell.Y + 1) / Height, 0f, 1f);

        // cellColorGetter ??= DefaultCellColorGetter;

        const int cellWidth = 16;
        const int cellHeight = 16;
        var line_thickness = 2;
        
        var cell_size = new SizeF(cellWidth, cellHeight);
        var image_width = Width * cellWidth;
        var image_height = Height * cellHeight;
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
                    var lt = new PointF(x * cellWidth, y * cellHeight);
                    var rgba32 = cellColorGetter(cell);
                    // var cell_color = Color.Blue;
                    var cell_color = new Color(rgba32);
                    // 先填充格子，然后填充格子 左边 和 上边 的线，保证线能够画在格子上面
                    // 因为格子的遍历顺序就是 从左往右 从上往下，之后这个格子上面的线和左边的 线都不会再次被格子本身覆盖掉
                    ctx.Fill(cell_color, new RectangleF(lt, cell_size));
                }
            }

            (PointF lt, PointF lb, PointF rb, PointF rt) GetCellPoints(int x, int y)
            {
                var lt = new PointF(x * cellWidth, y * cellHeight);
                var lb = new PointF(x * cellWidth, (y + 1) * cellHeight);
                var rb = new PointF((x + 1) * cellWidth, (y + 1) * cellHeight);
                var rt = new PointF((x + 1) * cellWidth, y * cellHeight);
                return (lt, lb, rb, rt);
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

                    var (lt, lb, rb, rt) = GetCellPoints(x, y);

                    if (!cell.HasLinkUp)
                        ctx.DrawLines(line_color, line_thickness, lt, rt);
                    if (!cell.HasLinkLeft)
                        ctx.DrawLines(line_color, line_thickness, lt, lb);
                }

                // Draw Right Line
                ctx.DrawLines(line_color, line_thickness, new PointF(image_width, 0),
                    new PointF(image_width, image_height));
                // Draw Bottom Line
                ctx.DrawLines(line_color, line_thickness, new PointF(0, image_height),
                    new PointF(image_width, image_height));
            }

            FillCellsColor();
            DrawAllCellsLines();
            // DrawCellLinesInFullGrid();
        });

        return image;
    }

    private struct InsetCellPoints
    {
        public int x0, x1, x2, x3;
        public int y0, y1, y2, y3;
    }

    public Image<Rgba32> DrawImageWithInset(Func<GridCell, Rgba32>? cellColorGetter = null)
    {
        const int cellWidth = 16;
        const int cellHeight = 16;
        const int inset = 2;
        const int innerCellWidth = cellWidth - 2 * inset;
        const int innerCellHeight = cellHeight - 2 * inset;
        
        var line_thickness = 1;
        
        var inner_cell_size = new SizeF(innerCellWidth, innerCellHeight);
        var image_width = Width * cellWidth;
        var image_height = Height * cellHeight;
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
                    var lt = new PointF(x * cellWidth + inset, y * cellHeight + inset);
                    var rgba32 = cellColorGetter(cell);
                    // var cell_color = Color.Blue;
                    var cell_color = new Color(rgba32);
                    // 先填充格子，然后填充格子 左边 和 上边 的线，保证线能够画在格子上面
                    // 因为格子的遍历顺序就是 从左往右 从上往下，之后这个格子上面的线和左边的 线都不会再次被格子本身覆盖掉
                    ctx.Fill(cell_color, new RectangleF(lt, inner_cell_size));
                    
                    var i = GetInsetCoordinate(x, y);
                    if (cell.HasLinkUp)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x1, i.y0, innerCellWidth, inset));
                    }
                    if (cell.HasLinkDown)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x1, i.y2, innerCellWidth, inset));
                    }
                    if (cell.HasLinkLeft)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x0, i.y1, inset, innerCellHeight));
                    }
                    if (cell.HasLinkRight)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x2, i.y1, inset, innerCellHeight));
                    }
                }
            }

            InsetCellPoints GetInsetCoordinate(int x, int y)
            {
                var i = new InsetCellPoints();
                
                i.x0 = x * cellWidth;
                i.x3 = (x + 1) * cellWidth;
                i.x1 = i.x0 + inset;
                i.x2 = i.x3 - inset;
                
                i.y0 = y * cellHeight;
                i.y3 = (y + 1) * cellHeight;
                i.y1 = i.y0 + inset;
                i.y2 = i.y3 - inset;
                
                return i;
            }

            void DrawLine(int x0, int y0, int x1, int y1) =>
                ctx.DrawLines(line_color, line_thickness, new PointF(x0, y0), new PointF(x1, y1));

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