using System.Diagnostics;
using SixLabors.ImageSharp.Drawing.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace PCG.Maze.MazeShape;

public class SimpleWeaveGrid : WeaveGrid
{
    public SimpleWeaveGrid(int width, int height) : base(width, height)
    {
    }

    public SimpleWeaveGrid(MaskGrid maskGrid) : base(maskGrid)
    {
    }

    // public override IEnumerable<GridCell> GetAllCells() => Cells.OfType<SimpleOverCell>().ToList();
    public override GridCell InitCell(int x, int y) => new SimpleOverCell(this, x, y);
}

public class WeaveGrid : Grid
{
    public WeaveGrid(int width, int height) : base(width, height)
    {
    }

    public WeaveGrid(MaskGrid maskGrid) : base(maskGrid)
    {
    }

    public List<UnderCell> UnderCells = new();
    public int OverCellCount => Width * Height;

    public override GridCell InitCell(int x, int y) => new OverCell(this, x, y);

    public void TunnelUnder(OverCell overCell)
    {
        var underCell = new UnderCell(overCell);
        UnderCells.Add(underCell);
    }

    /// <summary>
    /// 随机挖掘，对于每个 Cell，通过 UnionFind 来保证它的上下左右不是已经挖掘并连接过的
    /// </summary>
    /// <param name="random"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public void RandomTunnelUnder(Random random, int count)
    {
        // Debug.Assert(count < OverCellCount / 9);
        var union = new MazeGenerator.DisjointUnion<GridCell>();

        for (int i = 0; i < count; i++)
        {
            var x = random.Next(1, Width - 1);
            var y = random.Next(1, Height - 1);
            if (Cells[y, x] is not OverCell overCell) continue;
            var already_lined = overCell.GetLinks().Any();
            var can_union_ho = overCell.HasLeft && overCell.HasRight && !union.IsSameSet(overCell.Left, overCell.Right);
            var can_union_vertical = overCell.HasUp && overCell.HasDown && !union.IsSameSet(overCell.Up, overCell.Down);
            if (already_lined || !can_union_ho || !can_union_vertical) continue;

            var is_horizontal = random.NextSingle() < 0.5f;
            if (is_horizontal)
            {
                overCell.Link(overCell.Left, true);
                overCell.Link(overCell.Right, true);
                TunnelUnder(overCell);
                union.Union(overCell, overCell.Left);
                union.Union(overCell, overCell.Right);
            }
            else
            {
                overCell.Link(overCell.Up, true);
                overCell.Link(overCell.Down, true);
                TunnelUnder(overCell);
                union.Union(overCell, overCell.Up);
                union.Union(overCell, overCell.Down);
            }
        }
    }

    public override IEnumerable<GridCell> GetAllCells() => base.GetAllCells().Concat(UnderCells);

    public override Image<Rgba32> DrawImageWithInset(Func<GridCell, Rgba32>? cellColorGetter = null)
    {
        var image = InitImageToDraw();
        // 先画 UnderCell 的颜色，然后让基类把 OverCell 全给画了，最后来画 UnderCell 的线条
        image.Mutate(ctx =>
        {
            if (cellColorGetter != null)
            {
                foreach (var underCell in UnderCells)
                {
                    var i = GetInsetCoordinate(underCell.X, underCell.Y);

                    var rgba32 = cellColorGetter(underCell);
                    // var cell_color = Color.Blue;
                    var cell_color = new Color(rgba32);

                    if (underCell.IsVerticalPassage)
                    {
                        ctx.Fill(cell_color, new RectangleF(i.x1, i.y0, InnerCellWidth, Inset));
                        ctx.Fill(cell_color, new RectangleF(i.x1, i.y2, InnerCellWidth, Inset));
                    }
                    else
                    {
                        Debug.Assert(underCell.IsHorizontalPassage);
                        ctx.Fill(cell_color, new RectangleF(i.x0, i.y1, Inset, InnerCellHeight));
                        ctx.Fill(cell_color, new RectangleF(i.x2, i.y1, Inset, InnerCellHeight));
                    }
                }
            }
        });
        
        DrawImageWithInset(image, cellColorGetter);
        
        image.Mutate(ctx =>
        {
            void DrawLine(int x0, int y0, int x1, int y1) =>
                ctx.DrawLines(lineColor, lineThickness, new PointF(x0, y0), new PointF(x1, y1));


            foreach (var underCell in UnderCells)
            {
                var i = GetInsetCoordinate(underCell.X, underCell.Y);

                if (underCell.IsVerticalPassage)
                {
                    DrawLine(i.x1, i.y1, i.x1, i.y0);
                    DrawLine(i.x2, i.y1, i.x2, i.y0);

                    DrawLine(i.x1, i.y2, i.x1, i.y3);
                    DrawLine(i.x2, i.y2, i.x2, i.y3);
                }
                else
                {
                    Debug.Assert(underCell.IsHorizontalPassage);
                    DrawLine(i.x1, i.y1, i.x0, i.y1);
                    DrawLine(i.x1, i.y2, i.x0, i.y2);
                    DrawLine(i.x2, i.y1, i.x3, i.y1);
                    DrawLine(i.x2, i.y2, i.x3, i.y2);
                }
            }
        });
        return image;
    }
}