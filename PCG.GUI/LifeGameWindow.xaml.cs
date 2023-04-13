using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PCG.GUI;

public partial class LifeGameWindow : Window
{
    private const int LIVE = 1;
    private const int DEAD = 0;

    private const int CellSize = 10;

    private readonly int _rowCount;
    private readonly int _columnCount;
    private int[,] _cells;
    private readonly List<int[,]> _history;

    private readonly SolidColorBrush _aliveColorBrush = Brushes.Black;
    private readonly SolidColorBrush _deadColorBrush = Brushes.Transparent;

    private bool _isRunning = false;

    public float Chance { get; set; } = 100;
    public float Decay { get; set; } = 0.5f;
    private int colorId = 2;
    
    public LifeGameWindow() : this(64, 64)
    {
    }

    public LifeGameWindow(int rowCount, int columnCount)
    {
        InitializeComponent();
        _rowCount = rowCount;
        _columnCount = columnCount;
        _cells = new int[rowCount, columnCount];
        _history = new List<int[,]>();
        DrawGrid();
    }

    private Random random = new Random();

    private void DrawGrid()
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                Rectangle rect = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Fill = _deadColorBrush,
                };
                Canvas.SetLeft(rect, j * CellSize);
                Canvas.SetTop(rect, i * CellSize);
                CanvasPanel.Children.Add(rect);
            }
        }

        CanvasPanel.Width = _columnCount * CellSize;
        CanvasPanel.Height = _rowCount * CellSize;
    }

    private void UpdateGrid()
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                Rectangle rect = (Rectangle)CanvasPanel.Children[i * _columnCount + j];
                rect.Fill = ToBrush(_cells[i, j]);
            }
        }
    }

    private SolidColorBrush ToBrush(int value) =>
        value switch
        {
            DEAD => Brushes.Transparent,
            LIVE => Brushes.Black,
            2 => Brushes.DarkGray,
            3 => Brushes.PaleVioletRed,
            4 => Brushes.MediumVioletRed,
            5 => Brushes.MediumSpringGreen,
            6 => Brushes.MediumSlateBlue,
            7 => Brushes.LightSkyBlue,
            8 => Brushes.LightSalmon,
            9 => Brushes.LightSeaGreen,
            _ => Brushes.LightGreen,
        };

    private void NextGeneration()
    {
        int[,] newCells = new int[_rowCount, _columnCount];

        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                int count = CountNeighbors(i, j);
                if (_cells[i, j] == 1)
                {
                    newCells[i, j] = count >= 2 && count <= 3 ? LIVE : DEAD;
                }
                else
                {
                    newCells[i, j] = count == 3 ? LIVE : DEAD;
                }
            }
        }

        _history.Add((int[,])_cells.Clone());
        _cells.Initialize();
        Array.Copy(newCells, _cells, newCells.Length);
    }


    private void LazyFloodFill(int x, int y)
    {
        var queue = new Queue<(int Y, int X)>();
        queue.Enqueue((y, x));

        var chance = Chance;
        var decay = Decay;

        _history.Add((int[,])_cells.Clone());
        while (queue.Any())
        {
            var (cy, cx) = queue.Dequeue();
            _cells[cy, cx] = colorId;
            if (random.Next(100) > chance)
                continue;
            
            chance -= decay;
            var neighbors = GetAllNeighborsPosInCross(cx, cy)
                .Where(xy => _cells[xy.Y, xy.X] <= LIVE).Distinct();
            foreach (var xy in neighbors)
            {
                queue.Enqueue(xy);
            }
        }

        colorId++;
    }

    public IEnumerable<(int Y, int X)> GetAllNeighborsPosInCross(int x, int y)
    {
        if (x > 0)
        {
            var nx = x - 1;
            yield return (y, nx);
        }

        if (y > 0)
            yield return (y - 1, x);
        if (y < _rowCount - 1)
            yield return (y + 1, x);

        if (x < _columnCount - 1)
        {
            var nx = x + 1;
            yield return (y, nx);
        }
    }

    private int CountNeighbors(int row, int col)
    {
        int count = 0;

        for (int i = row - 1; i <= row + 1; i++)
        {
            for (int j = col - 1; j <= col + 1; j++)
            {
                if (i == row && j == col)
                {
                    continue;
                }

                int r = i < 0 ? _rowCount - 1 : i % _rowCount;
                int c = j < 0 ? _columnCount - 1 : j % _columnCount;

                if (_cells[r, c] == LIVE)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void RunButton_Click(object sender, RoutedEventArgs e)
    {
        _isRunning = !_isRunning;
        RunButton.Content = _isRunning ? "Stop" : "Run";
        while (_isRunning)
        {
            NextGeneration();
            UpdateGrid();
            System.Threading.Thread.Sleep(100);
            Application.Current.Dispatcher.Invoke(
                () => { },
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _history.Clear();
        _cells = new int[_rowCount, _columnCount];
        colorId = LIVE + 1;
        UpdateGrid();
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        if (_history.Count > 0)
        {
            _cells.Initialize();
            int[,] lastCells = _history[_history.Count - 1];
            Array.Copy(lastCells, _cells, lastCells.Length);
            _history.RemoveAt(_history.Count - 1);
            UpdateGrid();
        }
    }

    private void CanvasPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isRunning)
        {
            DrawCellByMouse(e);
        }
    }

    // 用于记录上次点亮的格子，避免触发 MouseMove 事件的时候，还在之前的格子却依然点亮了它
    private int prev_click_row = -1;
    private int prev_click_col = -1;

    private void DrawCellByMouse(MouseEventArgs e)
    {
        int row = (int)e.GetPosition(CanvasPanel).Y / CellSize;
        int col = (int)e.GetPosition(CanvasPanel).X / CellSize;
        if (prev_click_col == col && prev_click_row == row)
            return;
        prev_click_col = col;
        prev_click_row = row;
        
        LazyFloodFill(col, row);
        // _cells[row, col] = _cells[row, col] == LIVE ? DEAD : LIVE;
        
        UpdateGrid();
    }

    private void CanvasPanel_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isRunning && e.LeftButton == MouseButtonState.Pressed)
        {
            DrawCellByMouse(e);
        }
    }

    private void CanvasPanel_MouseCancelHold(object sender, MouseEventArgs e)
    {
        prev_click_col = -1;
        prev_click_row = -1;
    }
}