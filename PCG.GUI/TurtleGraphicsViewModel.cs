using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PCG.GUI;

public partial class TurtleGraphicsViewModel : ObservableObject
{
    public TurtleGraphicsWindow View { get; set; }
    [ObservableProperty] public string input = "";
    [ObservableProperty] public string recorded = "";
    [ObservableProperty] public int recursiveTime = 1;

    public Point CurrentPoint { get; set; } = new Point(40, 70);
    public Vector CurrentDirection { get; set; } = new(0, -1);
    [ObservableProperty] public double turnAngle = 90;

    [ObservableProperty] public int waitMS = 100;
    /// <summary>
    /// 用于停止 <see cref="RunByInput"/>
    /// </summary>
    [ObservableProperty] public bool isRunning = false;
    public Stack<Point> SavedPositions { get; set; } = new();

    public TurtleGraphicsViewModel(TurtleGraphicsWindow view)
    {
        View = view;
    }

    [ObservableProperty] public float distance = 50;

    public void RunByChar(char ch)
    {
        switch (ch)
        {
            case 'F':
                Forward();
                break;
            case '+':
                TurnRight();
                break;
            case '-':
                TurnLeft();
                break;
            case '[':
                SavePosition();
                break;
            case ']':
                LoadPosition();
                break;
        }
    }

    [RelayCommand]
    public void Reset()
    {
        ResetPos();
        isRunning = false;
        View.Reset();
    }

    public string RecursiveGen(string word, int count)
    {
        var tmp = word;

        for (int i = 0; i < count; i++)
            tmp = string.Concat(tmp.Select(c => c == 'F' ? Input : c.ToString()));

        return tmp;
    }

    [RelayCommand]
    public void RunByInput() => RunByString(RecursiveGen(Input, RecursiveTime));

    public void RunByString(string str)
    {
        IsRunning = true;
        foreach (var c in str.Cast<char>())
        {
            RunByChar(c);
            if (!IsRunning)
                return;
            if (WaitMS > 0)
                App.Wait(WaitMS);
            // yield return null;
        }
        IsRunning = false;
    }

    public void ResetPos()
    {
        CurrentPoint = new Point(View.Width / 2, View.Height / 2);
    }

    public void SavePosition()
    {
        SavedPositions.Push(CurrentPoint);
        View.UpdateHintLine();
    }

    public void LoadPosition()
    {
        if (!SavedPositions.Any())
            return;
        CurrentPoint = SavedPositions.Pop();
        View.UpdateHintLine();
    }

    public void TurnLeft() => TurnByAngle(-turnAngle);
    public void TurnRight() => TurnByAngle(turnAngle);

    public void Forward()
    {
        var start = CurrentPoint;
        var end = CurrentPoint + CurrentDirection * Distance;
        View.DrawLine(start, end);
        CurrentPoint = end;
        View.UpdateHintLine();
    }

    public void TurnByAngle(double angle)
    {
        var matrix = new Matrix();
        matrix.Rotate(angle);
        CurrentDirection = Vector.Multiply(CurrentDirection, matrix);
        View.UpdateHintLine();
    }
}