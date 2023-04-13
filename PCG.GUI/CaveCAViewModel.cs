using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static System.Threading.Thread;

#pragma warning disable MVVMTK0034

namespace PCG.GUI;

public partial class CaveCAViewModel : ObservableObject
{
    public MainWindow View { get; set; }

    [ObservableProperty] public int width = 128;
    [ObservableProperty] public int height = 84;

    [ObservableProperty] public double cavePercent = 0.6f;
    [ObservableProperty] public double toWallRatio = 0.55f;
    [ObservableProperty] public int seed = 0;

    [ObservableProperty] public int executionTimes = 16;

    [ObservableProperty] public CaveCA caveCa = new (0, 0, 0);

    [RelayCommand]
    public void Initialize()
    {
        InitializeCave();
        UpdateMap();
    }

    private void InitializeCave()
    {
        caveCa = new CaveCA(width, height, cavePercent)
        {
            ToWallRatio = toWallRatio,
            Seed = seed <= 0 ? new Random().Next() : seed
        };
        caveCa.Initialize();

        ConstructMap();
    }

    private void ConstructMap()
    {
        View.CavePanel.Children.Clear();

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var rectangle = new Rectangle()
                { Width = 8, Height = 8, Fill = ToBrush(CaveCell.Empty) };
            View.CavePanel.Children.Add(rectangle);
        }
    }

    private void UpdateMap()
    {
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            var index = y * width + x;
            var rect = (Rectangle)View.CavePanel.Children[index];
            rect.Fill = ToBrush(x, y);
        }
    }

    private SolidColorBrush ToBrush(CaveCell c) =>
        c switch
        {
            CaveCell.Empty => Brushes.DarkGray,
            CaveCell.Stone => Brushes.Black,
            CaveCell.Wall => Brushes.Brown,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        };

    private SolidColorBrush ToBrush(int x, int y)
        => caveCa.Map[y, x] switch
        {
            CaveCell.Empty =>
                caveCa.RoomMap[y, x] switch
                {
                    0 => Brushes.DarkGray,
                    1 => Brushes.PaleVioletRed,
                    2 => Brushes.MediumVioletRed,
                    3 => Brushes.MediumSpringGreen,
                    4 => Brushes.MediumSlateBlue,
                    5 => Brushes.LightSkyBlue,
                    6 => Brushes.LightSalmon,
                    7 => Brushes.LightSeaGreen,
                    _ => Brushes.LightGreen,
                },
            CaveCell.Stone => Brushes.Black,
            CaveCell.Wall => Brushes.Brown,
            CaveCell.ToDig => Brushes.Fuchsia,
            _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
        };

    [RelayCommand]
    public void Automata()
    {
        caveCa.Automation();
        UpdateMap();
    }

    [RelayCommand]
    public void DrawEdge()
    {
        caveCa.DrawEdge();
        UpdateMap();
    }

    [RelayCommand]
    public void FillRoom()
    {
        caveCa.FillRoom();
        UpdateMap();
    }

    [RelayCommand]
    public void ConnectRooms()
    {
        foreach (var _ in caveCa.ConnectRooms()) UpdateShow();
        return;
        
        caveCa.ConnectRooms();
        UpdateMap();
    }

    [RelayCommand]
    public void Execute()
    {
        InitializeCave();

        UpdateShow();
        for (int i = 0; i < ExecutionTimes; i++)
        {
            caveCa.Automation();
            UpdateShow();
        }

        caveCa.FillRoom();
        UpdateShow();

        foreach (var _ in caveCa.ConnectRooms())
        {
            UpdateShow();
        }

        caveCa.DrawEdge();
        UpdateMap();
    }

    private void UpdateShow()
    {
        UpdateMap();
        // return;
        Sleep(100);
        Application.Current.Dispatcher.Invoke(
            () => { },
            System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }
}