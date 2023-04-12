using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PCG.GUI;

public partial class LifeGameWindow : Window
{
    private const int CellSize = 10;

    private readonly int _rowCount;
    private readonly int _columnCount;
    private readonly bool[,] _cells;
    private readonly List<bool[,]> _history;

    private readonly SolidColorBrush _aliveColorBrush = Brushes.Black;
    private readonly SolidColorBrush _deadColorBrush = Brushes.Transparent;

    private bool _isRunning = false;
    public LifeGameWindow() : this(64, 64) {}

    public LifeGameWindow(int rowCount, int columnCount)
    {
        InitializeComponent();
        _rowCount = rowCount;
        _columnCount = columnCount;
        _cells = new bool[rowCount, columnCount];
        _history = new List<bool[,]>();
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
                    Fill = random.NextDouble() > 0.5? _deadColorBrush : _aliveColorBrush,
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
                rect.Fill = _cells[i, j] ? _aliveColorBrush : _deadColorBrush;
            }
        }
    }

    private void NextGeneration()
    {
        bool[,] newCells = new bool[_rowCount, _columnCount];

        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                int count = CountNeighbors(i, j);
                if (_cells[i, j])
                {
                    newCells[i, j] = count >= 2 && count <= 3;
                }
                else
                {
                    newCells[i, j] = count == 3;
                }
            }
        }

        _history.Add((bool[,])_cells.Clone());
        _cells.Initialize();
        Array.Copy(newCells, _cells, newCells.Length);
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

                if (_cells[r, c])
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
        _cells.Initialize();
        UpdateGrid();
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        if (_history.Count > 0)
        {
            _cells.Initialize();
            bool[,] lastCells = _history[_history.Count - 1];
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

    private void DrawCellByMouse(MouseEventArgs e)
    {
        int row = (int)e.GetPosition(CanvasPanel).Y / CellSize;
        int col = (int)e.GetPosition(CanvasPanel).X / CellSize;
        _cells[row, col] = !_cells[row, col];
        UpdateGrid();
    }

    private void CanvasPanel_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isRunning && e.LeftButton == MouseButtonState.Pressed)
        {
            DrawCellByMouse(e);
        }
    }
}
