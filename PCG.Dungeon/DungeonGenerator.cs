using System.Drawing;

namespace PCG.Dungeon;

public class DungeonGenerator
{
    public Represent Represent { get; private set; }
    public int Width => Represent.Width;
    public int Height => Represent.Height;

    public DungeonGenerator(Represent represent)
    {
        Represent = represent;
    }

    public DungeonGenerator(int width, int height, Represent represent) : this(represent)
    {
        represent.NewMap(width, height);
    }
}

public static class RepresentExtension
{
    public static void DrawRectangle(this Represent represent, Rectangle rect)
        => represent.DrawRectangle(rect.Left, rect.Top, rect.Width, rect.Height);

    public static void ValidateRectangle(this Represent represent, Rectangle rect)
    {
        represent.ValidateX(rect.Left);
        represent.ValidateX(rect.Right - 1);
        represent.ValidateX(rect.Top);
        represent.ValidateX(rect.Bottom - 1);
    }

    public static void Deconstruct(this Point point, out int x, out int y)
    {
        x = point.X;
        y = point.Y;
    }
}

public class BVHGenerator : DungeonGenerator
{
    public class BVHNode
    {
        public Rectangle Area;
        public Rectangle Fill;
        private SplitDir splitDir;
        public BVHNode? Left { get; private set; }
        public BVHNode? Right { get; private set; }
        public (Point left, Point right) Connection { get; private set; }

        enum SplitDir
        {
            Horizontal,
            Vertical
        };

        public bool IsLeaf => Left is null || Right is null;

        public Represent Represent { get; set; }

        public BVHNode(int width, int height, Represent represent)
            : this(new Rectangle(0, 0, width, height), represent)
        {
        }

        public BVHNode(Rectangle area, Represent represent)
        {
            Area = area;
            Represent = represent;
        }

        public BVHNode(Rectangle area, Rectangle fill, Represent represent)
            : this(area, represent)
        {
            Fill = fill;
        }

        public void Gen(int depth)
        {
            const int minAreaSize = 625;
            const int minWidth = 10;
            if (Area.Width * Area.Height < minAreaSize || Area.Width < minWidth || Area.Height < minWidth)
            {
                GenFill();
                return;
            }

            var aspect_ratio = ((double)Area.Width) / Area.Height;
            const double minAspectRatio = 0.8;
            const double maxAspectRatio = 1.2;
            splitDir = aspect_ratio switch
            {
                < minAspectRatio => SplitDir.Vertical,
                > maxAspectRatio => SplitDir.Horizontal,
                _ => Random.NextDouble() > 0.5 ? SplitDir.Vertical : SplitDir.Horizontal,
            };

            if (splitDir == SplitDir.Horizontal)
            {
                var (lt_padding, rb_padding) = GetPadding(Area.Width / 2);
                // TODO 此处的划分
                var split_x = Random.Next(Area.Left + lt_padding, Area.Right - rb_padding);
                var left_rect = new Rectangle(Area.Left, Area.Top, split_x - Area.Left, Area.Height);
                var right_rect = new Rectangle(split_x, Area.Top, Area.Right - split_x, Area.Height);

                Left = new BVHNode(left_rect, Represent);
                Right = new BVHNode(right_rect, Represent);
            }
            else
            {
                var (lt_padding, rb_padding) = GetPadding(Area.Width / 2);
                var split_y = Random.Next(Area.Top + lt_padding, Area.Bottom - rb_padding);
                var left_rect = new Rectangle(Area.Left, Area.Top, Area.Width, split_y - Area.Top);
                var right_rect = new Rectangle(Area.Left, split_y, Area.Width, Area.Bottom - split_y);

                Left = new BVHNode(left_rect, Represent);
                Right = new BVHNode(right_rect, Represent);
            }

            Left.Gen(depth - 1);
            Right.Gen(depth - 1);

            // 一定要在孩子 Gen 了以后，Fill 才生成出来，这个时候才能 Union
            // Tips: 碎片化地写代码，就会忘记这些细碎的生命周期
            Fill = Rectangle.Union(Left.Fill, Right.Fill);
        }

        private static (int lt_padding, int rb_padding) GetPadding(int max_padding)
        {
            const int minPadding = 1;
            const int maxPadding = 10;
            var padding = Random.Next(minPadding, Math.Min(maxPadding, max_padding));
            var lt_padding = Random.Next(minPadding, padding);
            var rb_padding = padding - lt_padding;
            return (lt_padding, rb_padding);
        }

        private void GenFill()
        {
            var (left_pad, right_pad) = GetPadding(Area.Width / 2);
            var (top_pad, bottom_pad) = GetPadding(Area.Height / 2);

            var left = Math.Min(Area.Left + left_pad, Area.Right - 1);
            var right = Math.Max(left + 1, Area.Right - right_pad);

            var top = Math.Min(Area.Top + top_pad, Area.Bottom - 1);
            var bottom = Math.Max(top + 1, Area.Bottom - bottom_pad);

            Fill = new Rectangle(left, top, right - left, bottom - top);
            Represent.DrawRectangle(Fill);
        }

        public void Connect()
        {
            if (IsLeaf) return;
            Left.Connect();
            Right.Connect();
            if (splitDir == SplitDir.Horizontal)
            {
                var top = Math.Min(Left.Fill.Top, Right.Fill.Top);
                var bottom = Math.Max(Left.Fill.Bottom, Right.Fill.Bottom);
                var y = Random.Next(top, bottom);
                Connection = (new Point(Left.Fill.Right + 1, y), new Point(Right.Fill.Left, y));
            }
            else
            {
                var left = Math.Min(Left.Fill.Left, Right.Fill.Left);
                var right = Math.Max(Left.Fill.Right, Right.Fill.Right);
                var x = Random.Next(left, right);
                Connection = (new Point(x, Left.Fill.Bottom + 1), new Point(x, Right.Fill.Top));
            }

            DrawConnection();
        }

        public void Draw()
        {
            if (IsLeaf)
            {
                Represent.DrawRectangle(Fill);
            }
            else
            {
                Left.Draw();
                Right.Draw();
                DrawConnection();
            }
        }

        private void DrawConnection()
        {
            var (x0, y0) = Connection.left;
            var (x1, y1) = Connection.right;
            Represent.DrawLine(x0, y0, x1, y1);
        }

        public static int GetPadding() => Random.Next(4, 10);
    }

    public static Random Random { get; private set; }
    public BVHNode RootNode { get; set; }

    static BVHGenerator()
    {
        var seed = new Random().Next();
        Random = new Random(seed);
    }

    public BVHGenerator(int width, int height, Represent represent)
        : base(width, height, represent)
    {
        RootNode = new BVHNode(width, height, represent);
    }

    public void Gen(int depth)
    {
        RootNode.Gen(depth);
        RootNode.Connect();
    }
}