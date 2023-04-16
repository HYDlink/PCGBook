using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PCG.GUI;

public partial class TurtleGraphicsWindow : Window
{
    private GeometryGroup geometryGroup;
    public const Key FORWARD_KEY = Key.W;
    public const Key TURN_LEFT_KEY = Key.A;
    public const Key TURN_RIGHT_KEY = Key.D;
    public const Key SAVE_POS_KEY = Key.Q;
    public const Key LOAD_POS_KEY = Key.E;

    private Line HintLine { get; set; }
    public TurtleGraphicsViewModel ViewModel => (TurtleGraphicsViewModel)DataContext;

    public TurtleGraphicsWindow()
    {
        DataContext = new TurtleGraphicsViewModel(this);
        InitializeComponent();
        Reset();
    }

    public void Reset()
    {
        TurtleCanvas.Children.Clear();
        AddHintLine();
        UpdateHintLine();
        InitLineGroupShape();
    }

    private void AddHintLine()
    {
        HintLine = new Line() 
            { Stroke = Brushes.Black, StrokeThickness = 2, StrokeDashOffset = 4 };
        Panel.SetZIndex(HintLine, 1000000);
        TurtleCanvas.Children.Add(HintLine);
    }

    public void UpdateHintLine()
    {
        var start = ViewModel.CurrentPoint;
        var end = ViewModel.CurrentPoint + ViewModel.CurrentDirection * ViewModel.Distance;

        HintLine.X1 = start.X;
        HintLine.Y1 = start.Y;
        HintLine.X2 = end.X;
        HintLine.Y2 = end.Y;
    }
    
    public void DrawLineDirectly(Point start, Point end)
    {
        var line = new Line()
        {
            X1 = start.X, Y1 = start.Y, X2 = end.X, Y2 = end.Y,
            Stroke = Brushes.PaleVioletRed, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        };
        TurtleCanvas.Children.Add(line);
    }

    // TODO Batch drawing instead of Shape Controls
    public void DrawLine(Point start, Point end)
    {
        geometryGroup.Children.Add(new LineGeometry(start, end));
    }

    private void InitLineGroupShape()
    {
        geometryGroup = new GeometryGroup();
        var path = new Path
        {
            Data = geometryGroup,
            Stroke = Brushes.PaleVioletRed, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
        };
        TurtleCanvas.Children.Add(path);
    }

    private void TurtleCanvas_OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case FORWARD_KEY:
                ViewModel.RecordRun('F');
                break;
            case TURN_RIGHT_KEY:
                ViewModel.RecordRun('+');
                break;
            case TURN_LEFT_KEY:
                ViewModel.RecordRun('-');
                break;
            case SAVE_POS_KEY:
                ViewModel.RecordRun('[');
                break;
            case LOAD_POS_KEY:
                ViewModel.RecordRun(']');
                break;
        }
    }

    private void TurtleCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        TurtleCanvas.Focus();
    }
}