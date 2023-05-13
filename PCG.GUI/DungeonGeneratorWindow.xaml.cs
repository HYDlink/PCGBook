using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PCG.Dungeon;
using Point = System.Windows.Point;

namespace PCG.GUI;

public partial class DungeonGeneratorWindow : Window
{
    public DungeonGeneratorViewModel ViewModel => (DungeonGeneratorViewModel)DataContext;

    public DungeonGeneratorWindow()
    {
        InitializeComponent();
        DataContext = new DungeonGeneratorViewModel(this);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        ViewModel.Width = (int)Canvas.ActualWidth;
        ViewModel.Height = (int)Canvas.ActualHeight;
    }
}

public class GeometryGroupRepresent : Represent
{
    public GeometryGroup RectGeometryGroup { get; set; }
    public GeometryGroup LineGeometryGroup { get; set; }
    public Canvas Canvas { get; set; }

    public GeometryGroupRepresent(Canvas canvas)
    {
        Canvas = canvas;
        RectGeometryGroup = new GeometryGroup();
        var rectPath = new Path
        {
            Data = RectGeometryGroup,
            // Stroke = Brushes.LightSeaGreen, StrokeThickness = 16,
            Fill = Brushes.Thistle,
            // StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        };
        LineGeometryGroup = new GeometryGroup();
        var linePath = new Path
        {
            Data = LineGeometryGroup,
            Stroke = Brushes.PaleVioletRed, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        };
        Canvas.Children.Add(rectPath);
        Canvas.Children.Add(linePath);
    }

    protected override void NewMapInternal(int width, int height)
    {
        ClearMap();
    }

    protected override void DrawPixelInternal(int x, int y)
    {
        RectGeometryGroup.Children.Add(new LineGeometry(new Point(x, y), new Point(x, y)));
        App.Wait(10);
    }

    public override void DrawLineInternal(int x1, int y1, int x2, int y2)
    {
        LineGeometryGroup.Children.Add(new LineGeometry(new Point(x1, y1), new Point(x2, y2)));
        App.Wait(10);
    }

    protected override void DrawRectangleInternal(int x, int y, int w, int h)
    {
        var rectangle_geometry = new RectangleGeometry(new Rect(x, y, w, h)) { RadiusX = 2, RadiusY = 2 };
        RectGeometryGroup.Children.Add(rectangle_geometry);
        App.Wait(10);
    }

    public override void ClearMap()
    {
        RectGeometryGroup.Children.Clear();
        LineGeometryGroup.Children.Clear();
    }
}