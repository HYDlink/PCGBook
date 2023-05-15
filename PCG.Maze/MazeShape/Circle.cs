using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace PCG.Maze.MazeShape;

public class Circle : IMazeMap<CircleCell>
{
    // public int CountPerRing { get; set; }
    public int CellWidth { get; set; }
    public int RingCount { get; set; }
    private float GetRadiusPerCell(int countPerRing) => 2f * MathF.PI / countPerRing;

    private CircleCell[][] Cells { get; set; }

    public Circle(int countPerRing, int ringCount)
    {
        // CountPerRing = countPerRing;
        // CellWidth = cellWidth;
        RingCount = ringCount;

        Cells = new CircleCell[ringCount][];

        // Init Cells
        for (int ring = 0; ring < RingCount; ring++)
        {
            // var inner_radius = ring * cellWidth;
            // var outer_radius = (ring + 1) * cellWidth;
            // var count_per_ring = countPerRing * ((int)Math.Pow(1, (ring / 2)));
            var count_per_ring = countPerRing;
            Cells[ring] = new CircleCell[count_per_ring];
            var radians_per_cell = 2f * MathF.PI / count_per_ring;
            for (int polar = 0; polar < count_per_ring; polar++)
            {
                var cell = new CircleCell(radians_per_cell * polar,
                    radians_per_cell * (polar + 1),
                    ring);
                Cells[ring][polar] = cell;
            }
        }

        SetAllCellsNeighbor();
    }

    private void SetAllCellsNeighbor()
    {
        for (int ring = 0; ring < RingCount; ring++)
        {
            // var inner_radius = ring * cellWidth;
            // var outer_radius = (ring + 1) * cellWidth;
            var count_in_ring = Cells[ring].Length;
            for (int polar = 0; polar < count_in_ring - 1; polar++)
            {
                var cell = Cells[ring][polar];
                if (ring + 1 < RingCount)
                {
                    var outer_ring = Cells[ring + 1];
                    var outer_cell = outer_ring[polar];
                    cell.Outer = outer_cell;
                    outer_cell.Inner = cell;
                }

                var ccw_cell = Cells[ring][polar + 1];
                cell.CounterClock = ccw_cell;
                ccw_cell.Clock = cell;
            }

            var start_cell = Cells[ring][0];
            var end_cell = Cells[ring][count_in_ring - 1];
            end_cell.CounterClock = start_cell;
            start_cell.Clock = end_cell;
        }
    }

    public IEnumerable<CircleCell> GetAllCells()
        => Cells.SelectMany(c => c);

    public void Print()
    {
        throw new NotImplementedException();
    }

    public Image<Rgba32> DrawImage(Func<CircleCell, Rgba32>? cellColorGetter = null)
    {
        int cell_width = 32;
        var width = cell_width * (2 * RingCount + 2);
        var image = new Image<Rgba32>(width, width);

        var line_thickness = 2;
        image.Mutate(ctx =>
        {
            // 先填充底色
            var base_color = Color.White;
            ctx.Fill(base_color, new RectangleF(0, 0, width, width));
            var center_x = (float)width / 2;
            var radius = cell_width * RingCount * 2;
            var center_point = new PointF(center_x, center_x);

            var line_color = Color.Black;

            void FillCellsColor()
            {
                for (var r = 0; r < RingCount; r++)
                {
                    var count_in_ring = Cells[r].Length;
                    for (var p = 0; p < count_in_ring; p++)
                    {
                        if (cellColorGetter is null) continue;
                        var cell = Cells[r][p];
                        if (cell is null) continue;
                        var rgba32 = cellColorGetter(cell);
                        // var cell_color = Color.Blue;
                        var cell_color = new Color(rgba32);

                        // 先填充格子，然后填充格子 左边 和 上边 的线，保证线能够画在格子上面
                        // 因为格子的遍历顺序就是 从左往右 从上往下，之后这个格子上面的线和左边的 线都不会再次被格子本身覆盖掉
                        // ctx.Fill(cell_color, new RectangleF(lt, cell_size));
                    }
                }
            }

            // DrawGridLines
            // 之前采用的是，从上往下从左往右，对每个网格只绘制它的左侧和上侧，最后再加上顶线和底线

            void DrawAllCellsLines()
            {
                for (var r = 0; r < RingCount; r++)
                {
                    var radius = r * cell_width;
                    var outer_radius = (r + 1) * cell_width;
                    var count_in_ring = Cells[r].Length;
                    var radians_per_cell = GetRadiusPerCell(count_in_ring);
                    for (var p = 0; p < count_in_ring; p++)
                    {
                        var cell = Cells[r][p];
                        if (cell is null) continue;

                        // i -> Inner, o -> Outer; cc -> CounterClockWise, c -> ClockWise
                        var (c_sin, c_cos) = MathF.SinCos(p * radians_per_cell);
                        var (cc_sin, cc_cos) = MathF.SinCos((p + 1) * radians_per_cell);
                        var ic = center_point + new PointF(radius * c_cos, radius * c_sin);
                        var icc = center_point + new PointF(radius * cc_cos, radius * cc_sin);
                        var oc = center_point + new PointF(outer_radius * c_cos, outer_radius * c_sin);
                        var occ = center_point + new PointF(outer_radius * cc_cos, outer_radius * cc_sin);

                        if (!cell.HasLinkCounterClock)
                            ctx.DrawLines(line_color, line_thickness, icc, occ);
                        if (!cell.HasLinkClock)
                            ctx.DrawLines(line_color, line_thickness, ic, oc);
                        // 外部曲线更宽，而外围还有更细的 Cell，这样的话会有重叠的线，因此在这里不画外侧
                        // if (!cell.HasLinkOuter)
                        //     ctx.DrawLines(line_color, line_thickness, oc, occ);
                        if (!cell.HasLinkInner)
                            ctx.DrawLines(line_color, line_thickness, ic, icc);
                    }
                }

                var ellipse_polygon = new EllipsePolygon(center_x, center_x, radius, radius);
                ctx.Draw(line_color, line_thickness, ellipse_polygon);
            }

            DrawAllCellsLines();
        });

        return image;
    }
}