using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PCG.GUI;

public partial class CaveCAViewModel : ObservableObject
{
    [ObservableProperty]
    public int width = 128;
    [ObservableProperty]
    public int height = 84;

    [ObservableProperty] public double cavePercent = 0.5f;
    [ObservableProperty] public double toWallRatio = 0.5f;
    [ObservableProperty] public int seed = 0;
    
    [ObservableProperty]
    public int executionTimes = 16;
    
    [ObservableProperty]
    public List<SolidColorBrush> map;
    
    [ObservableProperty]
    public CaveCA caveCa;
    
    [RelayCommand]
    public void Initialize()
    {
        InitializeCave();
        UpdateMap();
    }

    private void InitializeCave()
    {
        caveCa = new CaveCA(width, height, cavePercent);
        caveCa.ToWallRatio = toWallRatio;
        caveCa.Seed = seed <= 0 ? new Random().Next() : seed;
        caveCa.Initialize();
    }

    private void UpdateMap()
    {
        Map = caveCa.Map.Cast<CaveCell>().Select(c => c switch
        {
            CaveCell.Empty => Brushes.DarkGray,
            CaveCell.Stone => Brushes.Black,
            CaveCell.Wall => Brushes.Brown,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        }).ToList();
    }

    [RelayCommand]
    public void Automata()
    {
        if (caveCa is null)
            return;
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
    public void Execute()
    {
        InitializeCave();
        for (int i = 0; i < ExecutionTimes; i++) caveCa.Automation();
        caveCa.DrawEdge();
        UpdateMap();
    }
}