﻿using PCG.Maze.MazeShape;

namespace PCG.Maze.ValueMap;

public record DeadEndMap(int Width, int Height) : Map2D(Width, Height)
{
    public Grid Grid { get; init; }
    public List<GridCell> DeadEnds { get; set; }
    public int DeadEndsCount => DeadEnds.Count;

    public static DeadEndMap GetDeadEndMap(Grid grid)
    {
        var height = grid.Height;
        var width = grid.Width;
        var map = new DeadEndMap(width, height) { Grid = grid };

        var dead_ends = map.DeadEnds = grid.GetDeadEnds().ToList();
        foreach (var dead_end in dead_ends)
        {
            map[dead_end] = DeadEndValue;
        }

        foreach (var dead_end in dead_ends)
        {
            foreach (var corridor in dead_end.GetDeadEndCorridor())
            {
                map[corridor] = DeadEndCorridorValue;
            }
        }

        return map;
    }

    private const int NothingValue = 0;
    private const int DeadEndValue = 1;
    private const int DeadEndCorridorValue = 2;
    public static readonly Rgba32 DeadEndColor = new Rgba32(1f, 0, 1f);
    public static readonly Rgba32 DeadEndCorridorColor = new Rgba32(0.5f, 0, 1f);
    public static readonly Rgba32 NothingColor = new Rgba32(0.5f, 0, 0f, 0f);

    public Func<GridCell, Rgba32> GetCellColorGetter()
    {
        Rgba32 GetColor(GridCell cell)
        {
            return this[cell] switch
            {
                NothingValue => NothingColor,
                DeadEndValue => DeadEndColor,
                DeadEndCorridorValue => DeadEndCorridorColor,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return GetColor;
    }
}