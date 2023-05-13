using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using PCG.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace PCG.Maze;

public class Grid
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Cell[,] Cells { get; set; }

    public Grid(int width, int height)
    {
        Width = width;
        Height = height;

        Debug.Assert(Width >= 2 && Height >= 2);

        Cells = new Cell[height, width];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            Cells[y, x] = new Cell(x, y);

        // 设置 Cell 的邻居方向
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            if (x + 1 < Width)
            {
                Cells[y, x].Right = Cells[y, x + 1];
                Cells[y, x + 1].Left = Cells[y, x];
            }

            if (y + 1 < Height)
            {
                Cells[y, x].Down = Cells[y + 1, x];
                Cells[y + 1, x].Up = Cells[y, x];
            }
        }
    }

    public static Cell GetRandomNeighbor(Cell cell, Random random)
    {
        var array = cell.GetNeighbors().ToArray();
        return array[random.Next(array.Length)];
    }

    public string CellToString(Cell cell) => "    ";

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

    public Image<Rgba32> DrawImage(Func<Cell, Rgba32>? cellColorGetter = null)
    {
        Rgba32 defaultCellColorGetter(Cell cell) =>
            new Rgba32((float)(cell.X + 1) / Width, (float)(cell.Y + 1) / Height, 0f, 1f);

        cellColorGetter ??= defaultCellColorGetter;

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

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cells[y, x];

                    var lt = new PointF(x * cellWidth, y * cellHeight);
                    var lb = new PointF(x * cellWidth, (y + 1) * cellHeight);
                    var rb = new PointF((x + 1) * cellWidth, (y + 1) * cellHeight);
                    var rt = new PointF((x + 1) * cellWidth, y * cellHeight);

                    var is_linked_left = cell.HasLeft && cell.IsLinked(cell.Left);
                    var is_linked_up = cell.HasUp && cell.IsLinked(cell.Up);

                    var rgba32 = cellColorGetter(cell);
                    // var cell_color = Color.Blue;
                    var cell_color = new Color(rgba32);

                    // 先填充格子，然后填充格子 左边 和 上边 的线，保证线能够画在格子上面
                    // 因为格子的遍历顺序就是 从左往右 从上往下，之后这个格子上面的线和左边的 线都不会再次被格子本身覆盖掉
                    ctx.Fill(cell_color, new RectangleF(lt, cell_size));

                    if (!is_linked_up) ctx.DrawLines(line_color, line_thickness, lt, rt);

                    if (!is_linked_left)
                        ctx.DrawLines(line_color, line_thickness, lt, lb);
                }
            }

            // Top and Bottom Line
            ctx.DrawLines(line_color, line_thickness, new PointF(0, 0), new PointF(image_width, 0));
            ctx.DrawLines(line_color, line_thickness, new PointF(0, image_height),
                new PointF(image_width, image_height));
        });

        return image;
        // Utilities.SaveImage(image, "Maze");
        // image.Dispose();
    }
}