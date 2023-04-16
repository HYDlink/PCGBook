using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PCG.GUI;

public partial class TurtleGraphicsViewModel : ObservableObject
{
    public static readonly Vector DefaultDirection = new(0, -1);

    public TurtleGraphicsWindow View { get; }

    [ObservableProperty] public string input = "";
    [ObservableProperty] public string recorded = "";
    [ObservableProperty] public int recursiveTime = 1;

    [ObservableProperty] public double turnAngle = 90;

    [ObservableProperty] public int waitMs = 100;

    /// <summary>
    /// 用于停止 <see cref="RunByInput"/>
    /// </summary>
    public bool isRunning = false;

    [ObservableProperty] public float distance = 50;

    public Point CurrentPoint { get; set; } = new(40, 70);
    public Vector CurrentDirection { get; set; } = DefaultDirection;

    public record struct PosAndDir(Point Pos, Vector Dir);

    public Stack<PosAndDir> SavedPositions { get; set; } = new();

    public TurtleGraphicsViewModel(TurtleGraphicsWindow view)
    {
        View = view;
        executor = new Dictionary<char, Action>()
        {
            { 'F', Forward },
            { '+', TurnRight },
            { '-', TurnLeft },
            { '[', SavePosition },
            { ']', LoadPosition },
        };
    }

    private readonly Dictionary<char, Action> executor;

    [RelayCommand]
    public void RunByInput() => RunByString(RecursiveGen(Input, RecursiveTime));

    [RelayCommand]
    public void RunByRecorded() => RunByString(RecursiveGen(Recorded, RecursiveTime));

    [RelayCommand]
    private void CopyRecorded()
    {
        Clipboard.SetText(Recorded);
    }

    [RelayCommand]
    private void ResetAll()
    {
        Reset();
        ResetRecorded();
    }

    public event Action OnStopRunning;

    public static Action RegisterOnce(Action target, Action toRegister)
    {
        void Once()
        {
            toRegister?.Invoke();
            target -= Once;
        }

        target += Once;
        return target;
    }

    /// <summary>
    /// 重置状态和视图，但是不会重设 <see cref="recorded"/>，不会清空撤消重做栈
    /// </summary>
    private void Reset()
    {
        if (isRunning)
        {
            OnStopRunning = RegisterOnce(OnStopRunning, ResetState);
            isRunning = false;
        }
        else
        {
            ResetState();
        }
    }

    private void ResetState()
    {
        CurrentPoint = new Point(View.ActualWidth / 2, View.ActualHeight / 2);
        CurrentDirection = new Vector(0, -1);
        SavedPositions.Clear();
        View.Reset();
    }

    private void ResetRecorded()
    {
        Recorded = "";
        undoStack.Clear();
        redoStack.Clear();
    }

    #region Execution

    public string RecursiveGen(string word, int count)
    {
        var tmp = word;

        for (int i = 0; i < count; i++)
            tmp = string.Concat(tmp.Select(c => c == 'F' ? Input : c.ToString()));

        return tmp;
    }

    public void RunByChar(char ch)
    {
        if (executor.TryGetValue(ch, out var action))
            action();
    }

    public void RunByString(string str, bool wait = true)
    {
        isRunning = true;
        foreach (var c in str.Cast<char>())
        {
            RunByChar(c);
            if (!isRunning)
            {
                OnStopRunning?.Invoke();
                return;
            }

            if (wait && WaitMs > 0)
                App.Wait(WaitMs);
            // yield return null;
        }

        isRunning = false;
    }

    #endregion

    #region Record Undo Redo

    private record Recorder(string RecordInput, Point CurrentPoint, Vector CurrentDirection,
        Stack<PosAndDir> SavedPosition);

    private Stack<Recorder> undoStack = new();
    private Stack<Recorder> redoStack = new();

    private Recorder SaveRecorder() =>
        new Recorder(Recorded, CurrentPoint, CurrentDirection, CloneStack(SavedPositions));

    private void LoadRecorder(Recorder recorder)
    {
        Recorded = recorder.RecordInput;
        CurrentPoint = recorder.CurrentPoint;
        CurrentDirection = recorder.CurrentDirection;
        SavedPositions = CloneStack(recorder.SavedPosition);
    }

    public static Stack<T> CloneStack<T>(Stack<T> stack) where T : struct
    {
        var ns = new Stack<T>();
        foreach (var x1 in stack) ns.Push(x1);

        return ns;
    }

    /// <summary>
    /// 重新画一遍之前录制的过程，因此需要把整个绘画状态重置为初始然后开始画画
    /// </summary>
    private void UpdateView()
    {
        Reset();
        RunByString(Recorded, false);
    }

    public void SaveUndo()
    {
        var save_recorder = SaveRecorder();
        undoStack.Push(save_recorder);
        redoStack.Clear();
    }

    [RelayCommand]
    public void Undo()
    {
        if (!undoStack.TryPop(out var undo)) return;
        redoStack.Push(undo);
        LoadRecorder(undo);
        UpdateView();
    }

    [RelayCommand]
    public void Redo()
    {
        if (!redoStack.TryPop(out var redo)) return;
        undoStack.Push(redo);
        LoadRecorder(redo);
        UpdateView();
    }

    public void RecordRun(char ch)
    {
        SaveUndo();
        Recorded += ch;
        RunByChar(ch);
    }

    #endregion

    #region Drawing

    public void SavePosition()
    {
        SavedPositions.Push(new(CurrentPoint, CurrentDirection));
        View.UpdateHintLine();
    }

    public void LoadPosition()
    {
        if (!SavedPositions.Any())
            return;
        (CurrentPoint, CurrentDirection) = SavedPositions.Pop();
        View.UpdateHintLine();
    }

    public void TurnLeft() => TurnByAngle(-TurnAngle);
    public void TurnRight() => TurnByAngle(TurnAngle);

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

    #endregion
}