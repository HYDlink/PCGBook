﻿// See https://aka.ms/new-console-template for more information

using PCG.Common;
using PCG.Maze;
using PCG.Maze.MazeShape;
using PCG.Maze.Modifier;
using PCG.Maze.ValueMap;
using static PCG.Maze.MazeShape.MaskGrid;
using static PCG.Maze.MazeGenerator;

void MaskProgram()
{
    var ascii_mask = MaskGrid.FromString(@"X........X
....XX....
...XXXX...
....XX....
X........X
X........X
....XX....
...XXXX...
....XX....
X........X");

// TestMask(ascii_mask);
// var image_mask = MaskGrid.FromImage(@"C:\Work\Projects\maze-code\maze_text.png");
    DrawTextToImage("Doppo.");
    TestMask(MaskGrid.FromImage(DrawTextToImage("Cookies.")));
    TestMask(MaskGrid.FromImage(DrawTextToImage("小鸟")));

    void TestMask(MaskGrid maskGrid)
    {
        var grid = new Grid(maskGrid);
// mg.DrawImage().SaveImage("MaskedMaze");
        BackTrackLink(grid);
        grid.DrawImage().SaveImage("MaskedMaze");
        // var distance_value = DistanceMap.GetDistanceMap(grid, grid.GetAllCells().First());
        // grid.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("MaskedMaze");
    }
}

void DeadEndProgram()
{
    var grid = new Grid(8, 8);
    // RecordGif(generator);
    BackTrackLink(grid);
    var map = DeadEndMap.GetDeadEndMap(grid);
    grid.DrawImage(map.GetCellColorGetter()).SaveImage("Maze");
}

void DeadEndRemovalProgram()
{
    var (width, height) = (64, 64);
    var grid = new Grid(width, height);
    
    BackTrackLink(grid);
    grid.RemoveDeadEndPath(0.1f);
    // var map = DeadEndMap.GetDeadEndMap(grid);
    // grid.DrawImage(map.GetCellColorGetter()).SaveImage("Maze");
    grid.DrawImage().SaveImage("Maze");
}

void RecordGif<TCell>(IMazeMap<TCell> grid) where TCell : CellBase
{
    var images = new List<Image<Rgba32>>();
    BackTrackLink(grid, StepDrawImage);

    void StepDrawImage(IMazeMap<TCell> grid)
    {
        var draw_image = grid.DrawImage();
        images.Add(draw_image);
    }

    images.EncodeGif();
}

void TestGridAndDistanceMap()
{
    var (width, height) = (64, 64);
    var grid = new Grid(width, height);
    
    BackTrackLink(grid);
    grid.RemoveDeadEnd(0.5f);

    var distance_value = DistanceMap.GetDistanceMap(grid, 1, 1);
    var path_value = distance_value.GetPathMap(grid.Cells[width - 1, height - 1]);
    grid.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("DistanceInGridMaze");
    grid.DrawImage(path_value.GetCellColorByDistanceValue(true)).SaveImage("ShortestPathInGridMaze");
}

void TestCircle()
{
    var circle = new Circle(6, 4);
    BackTrackLink(circle);
    circle.DrawImage().SaveImage("CircleMaze");
}

// TestGridAndDistanceMap();
TestCircle();
// DeadEndProgram();
// DeadEndRemovalProgram();