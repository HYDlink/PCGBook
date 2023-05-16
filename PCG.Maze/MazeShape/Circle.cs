using System.Diagnostics;
using PCG.Common;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using Path = SixLabors.ImageSharp.Drawing.Path;

namespace PCG.Maze.MazeShape;

public class Circle : IMazeMap<CircleCell>
{
    // public int CountPerRing { get; set; }
    public int CellWidth { get; set; }
    public int RingCount { get; set; }
    private float GetRadiusPerCell(int countPerRing) => 2f * MathF.PI / countPerRing;

    private CircleCell?[][] Cells { get; set; }

    public CircleCell? GetCell(int ringIndex, int polarIndex) => Cells[ringIndex]?[polarIndex];

    /// <summary>
    /// 创建带有 Subdivision 的圆环迷宫
    /// </summary>
    /// <param name="countPerRing">最内侧环的每个 Cell 的数量</param>
    /// <param name="ringCount">推荐 ringCount 不要大于 10，因为环的 Cell 数量，每往外 2 个环，就会 x2 增长，是幂增长；<br/>
    /// 比如最内侧是 5 个 Cell，第 0 1 层都是 5 个 Cell，2 3 层是 5 * 2 = 10 个 Cell，<br/>
    /// 第 10 层就是 5 * (2 ^ 5) = 160 个 Cell</param>
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

            // 这里开始 subdivision
            var count_per_ring = countPerRing * ((int)Math.Pow(2, (ring / 2)));
            // var count_per_ring = countPerRing;
            Cells[ring] = new CircleCell[count_per_ring];
            var radians_per_cell = 2f * MathF.PI / count_per_ring;
            for (int polar = 0; polar < count_per_ring; polar++)
            {
                var cell = new CircleCell(polar, ring);
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
            for (int polar = 0; polar < count_in_ring; polar++)
            {
                var cell = Cells[ring][polar];
                if (cell is null) continue;
                // 因为外侧的 Cell 比里侧的多，如果是和外侧链接的话，那有很多的外侧邻居需要处理，所以反过来是让外侧向里侧连接
                if (ring - 1 >= 0)
                {
                    var inner_ring = Cells[ring];
                    var inner_cell = inner_ring[polar / 2];
                    if (inner_cell is not null)
                    {
                        inner_cell.OuterNeighbors.Add(cell);
                        cell.Inner = inner_cell;
                    }
                }

                var ccw_cell = Cells[ring][(polar + 1) % count_in_ring];
                if (ccw_cell is not null)
                {
                    cell.CounterClock = ccw_cell;
                    ccw_cell.Clock = cell;
                }
            }
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
            var line_color = Color.Black;
            ctx.Fill(base_color, new RectangleF(0, 0, width, width));
            
            var center_x = (float)width / 2;
            var circle_radius = cell_width * RingCount * 2;
            var center_point = new PointF(center_x, center_x);


            // i -> Inner, o -> Outer; cc -> CounterClockWise, c -> ClockWise
            (PointF ic, PointF icc, PointF oc, PointF occ) GetCellPoints(int polar, float radiansPerCell, int radius,
                int outerRadius)
            {
                var (c_sin, c_cos) = MathF.SinCos(polar * radiansPerCell);
                var (cc_sin, cc_cos) = MathF.SinCos((polar + 1) * radiansPerCell);
                var pointF = center_point + new PointF(radius * c_cos, radius * c_sin);
                var icc1 = center_point + new PointF(radius * cc_cos, radius * cc_sin);
                var oc1 = center_point + new PointF(outerRadius * c_cos, outerRadius * c_sin);
                var occ1 = center_point + new PointF(outerRadius * cc_cos, outerRadius * cc_sin);
                return (pointF, icc1, oc1, occ1);
            }

            ArcLineSegment GetArc(PointF @from, PointF to, float radius) =>
                new(from, to, radius: new SizeF(radius, radius), 
                    rotation: 0, largeArc: false, sweep: true);

            void FillCellsColor()
            {
                if (cellColorGetter is null) return;
                
                // For test
                // var random = Utilities.CreateRandomWithPrintedSeed();
                for (var r = 0; r < RingCount; r++)
                {
                    var radius = r * cell_width;
                    var outer_radius = (r + 1) * cell_width;
                    var count_in_ring = Cells[r].Length;
                    var radians_per_cell = GetRadiusPerCell(count_in_ring);
                    for (var p = 0; p < count_in_ring; p++)
                    {
                        // For test
                        // if (random.NextSingle() < 0.5f) continue;
                        // var cell_color = Color.Blue;
                        
                        var cell = Cells[r][p];
                        if (cell is null) continue;
                        var rgba32 = cellColorGetter(cell);
                        var cell_color = new Color(rgba32);

                        // 先填充格子，然后填充格子 左边 和 上边 的线，保证线能够画在格子上面
                        // 因为格子的遍历顺序就是 从左往右 从上往下，之后这个格子上面的线和左边的 线都不会再次被格子本身覆盖掉
                        // ctx.Fill(cell_color, new RectangleF(lt, cell_size));

                        var (ic, icc, oc, occ) = GetCellPoints(p, radians_per_cell, radius, outer_radius);

                        var inner_arc = GetArc(ic, icc, radius);
                        var outer_arc = GetArc(oc, occ, outer_radius);
                        var path = new Path(new ILineSegment[]
                            { inner_arc, new LinearLineSegment(icc, occ), outer_arc, new LinearLineSegment(oc, ic) });
                        ctx.Fill(cell_color, path);
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

                        var (ic, icc, oc, occ) = GetCellPoints(p, radians_per_cell, radius, outer_radius);

                        if (!cell.HasLinkCounterClock)
                            ctx.DrawLines(line_color, line_thickness, icc, occ);
                        if (!cell.HasLinkClock)
                            ctx.DrawLines(line_color, line_thickness, ic, oc);
                        // 外部曲线更宽，而外围还有更细的 Cell，这样的话会有重叠的线，因此在这里不画外侧
                        // if (!cell.HasLinkOuter)
                        //     ctx.DrawLines(line_color, line_thickness, oc, occ);
                        if (!cell.HasLinkInner)
                            // ctx.DrawLines(line_color, line_thickness, ic, icc);
                        {
                            var arc = GetArc(ic, icc, radius);
                            ctx.Draw(line_color, line_thickness, new Path(arc));
                        }
                    }
                }

                var ellipse_polygon = new EllipsePolygon(center_x, center_x, circle_radius, circle_radius);
                ctx.Draw(line_color, line_thickness, ellipse_polygon);
            }

            FillCellsColor();
            DrawAllCellsLines();
        });

        return image;
    }

    public void RemoveCell(CircleCell cell)
    {
        Debug.Assert(Cells[cell.RingIndex][cell.PolarIndex] == cell);
        Cells[cell.RingIndex][cell.PolarIndex] = null;
        
        // tackle with OtherNeighbors
        if (cell.Inner != null) cell.Inner.OuterNeighbors.Remove(cell);
        // if (cell.Outer != null) cell.Outer.Inner = null;
        cell.OuterNeighbors.ForEach(outer => outer.Inner = null);
        
        if (cell.CounterClock != null) cell.CounterClock.Clock = null;
        if (cell.Clock != null) cell.Clock.CounterClock = null;

        foreach (var grid_cell in cell.GetLinks())
        {
            cell.UnLink(grid_cell, true);
        }
    }
}