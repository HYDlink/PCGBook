// See https://aka.ms/new-console-template for more information

using System.Numerics;
using PCG.Common;
using PCG.Maze;
using PCG.Maze.MazeShape;
using PCG.Maze.MazeShape.Advanced;
using PCG.Maze.Modifier;
using PCG.Maze.ValueMap;
using Raylib_cs;
using static PCG.Maze.MazeShape.MaskGrid;
using static PCG.Maze.MazeGenerator;
using Color = SixLabors.ImageSharp.Color;

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
    TestMask(FromImage(DrawTextToImage("Cookies.")));
    TestMask(FromImage(DrawTextToImage("小鸟")));
    TestMask(FromImage(DrawTextToImage("乐观的食用盐")));

    void TestMask(MaskGrid maskGrid)
    {
        var grid = new Grid(maskGrid);
// mg.DrawImage().SaveImage("MaskedMaze");
        BackTrackLink(grid);
        grid.DrawImage().SaveImage("MaskedMaze");
        var distance_value = new DistanceMap<GridCell>(grid, grid.GetAllCells().First());
        grid.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("MaskedMaze");
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
    var (width, height) = (32, 32);
    var grid = new Grid(width, height);

    AldousBroderLink(grid);
    grid.RemoveDeadEndPath(0.5f);
    // var map = DeadEndMap.GetDeadEndMap(grid);
    // grid.DrawImageWithInset(map.GetCellColorGetter()).SaveImage("Maze");

    var distance_value = new DistanceMap<GridCell>(grid, grid.GetAllCells().RandomItem(new Random()));
    grid.DrawImageWithInset().SaveImage("Maze");
    grid.DrawImageWithInset(distance_value.GetCellColorByDistanceValue()).SaveImage("Maze");
}

void RecordGif<TCell>(IMazeMap<TCell> grid, Generator<TCell> generator) where TCell : CellBase
{
    var images = new List<Image<Rgba32>>();
    generator(grid, StepDrawImage);

    void StepDrawImage(IMazeMap<TCell> grid)
    {
        var draw_image = grid.DrawImage();
        images.Add(draw_image);
    }

    images.EncodeGif(100);
}

void RecordGifForGrid(Grid grid, Action<Grid, Action<Grid>> generator)
{
    var images = new List<Image<Rgba32>>();
    generator(grid, StepDrawImage);

    void StepDrawImage(Grid grid)
    {
        var draw_image = grid.DrawImage();
        images.Add(draw_image);
    }

    images.EncodeGif(10);
}

void TestGridAndDistanceMap()
{
    var (width, height) = (32, 32);
    var grid = new Grid(width, height);
    // grid.DrawImageWithInset().SaveImage("Maze");
    // return;

    EllerOnGrid(grid);
    grid.DrawImageWithInset().SaveImage("GridMaze");
    // RecordGifForGrid(grid, Eller);
    // return;
    // grid.RemoveDeadEndPath(0.5f);

    var rand = Utilities.CreateRandomWithPrintedSeed();
    var startCell = grid.GetAllCells().RandomItem(rand);
    var endCell = grid.GetAllCells().RandomItem(rand);
    var distance_value = new DistanceMap<GridCell>(grid, startCell);
    var path_value = distance_value.GetPathMap(endCell);
    grid.DrawImageWithInset(distance_value.GetCellColorByDistanceValue()).SaveImage("DistanceInGridMaze");
    grid.DrawImageWithInset(path_value.GetCellColorByDistanceValue(true)).SaveImage("ShortestPathInGridMaze");
}

void TestCircle()
{
    var circle = new Circle(6, 4);
    BackTrackLink(circle);
    circle.DrawImage().SaveImage("CircleMaze");
    var distance_value = new DistanceMap<CircleCell>(circle, circle.GetCell(1, 1));
    var path_value = distance_value.GetPathMap(circle.GetCell(3, 1));
    circle.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("DistanceInCircleMaze");
    circle.DrawImage(path_value.GetCellColorByDistanceValue(true)).SaveImage("ShortestPathInCircleMaze");
}

void TestWeaveGrid()
{
    var (width, height) = (32, 32);
    var grid = new SimpleWeaveGrid(width, height);
    grid.RandomTunnelUnder(Utilities.CreateRandomWithPrintedSeed(), 32 * 32);
    grid.DrawImageWithInset().SaveImage("Maze");
    // return;

    KruskalLink(grid);
    grid.DrawImageWithInset().SaveImage("GridMaze");

    var rand = Utilities.CreateRandomWithPrintedSeed();
    var startCell = grid.GetAllCells().RandomItem(rand);
    var endCell = grid.GetAllCells().RandomItem(rand);
    var distance_value = new DistanceMap<GridCell>(grid, startCell);
    var path_value = distance_value.GetPathMap(endCell);
    grid.DrawImageWithInset(distance_value.GetCellColorByDistanceValue()).SaveImage("DistanceInGridMaze");
    grid.DrawImageWithInset(path_value.GetCellColorByDistanceValue(true)).SaveImage("ShortestPathInGridMaze");
}

void TestHexGrid()
{
    var (width, height) = (32, 32);
    var grid = new HexGrid(width, height);
    // grid.DrawImage().SaveImage("Maze");
    // return;

    KruskalLink(grid);
    // grid.DrawImage().SaveImage("GridMaze");

    var rand = Utilities.CreateRandomWithPrintedSeed();
    var startCell = grid.GetAllCells().RandomItem(rand);
    var endCell = grid.GetAllCells().RandomItem(rand);
    var distance_value = new DistanceMap<HexCell>(grid, startCell);
    var path_value = distance_value.GetPathMap(endCell);
    grid.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("DistanceInGridMaze");
    // grid.DrawImage(path_value.GetCellColorByDistanceValue(true)).SaveImage("ShortestPathInGridMaze");
}

void TestNormalGrid()
{
    var (width, height) = (32, 32);
    var grid = new Grid(width, height);
    // grid.DrawImageWithInset().SaveImage("Maze");
    // return;

    RecursiveDivision(grid);
    grid.DrawImage().SaveImage("GridMaze");
    var rand = Utilities.CreateRandomWithPrintedSeed();
    var startCell = grid.GetAllCells().RandomItem(rand);
    var endCell = grid.GetAllCells().RandomItem(rand);
    var distance_value = new DistanceMap<GridCell>(grid, startCell);
    var path_value = distance_value.GetPathMap(endCell);
    grid.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("DistanceInGridMaze");
    grid.DrawImage(path_value.GetCellColorByDistanceValue(true)).SaveImage("ShortestPathInGridMaze");
}

void TestRayLib()
{
    var grid3D = new Grid3D(4, 4, 4);
    KruskalLink(grid3D);
    new Grid3DPlayer(grid3D).Play();
    // grid3D.DrawRayLib();
}

void TestCylinder()
{
    var grid = new CylinderGrid(32, 32);
    KruskalLink(grid);
    var file_name = "CylinderGridDistance";
    var file_path = file_name + ".png";
    // var file_path = @"C:\Work\Projects\PCGBook\PCG.Maze\bin\Debug\net7.0\01-uv-texture.png";
    // if (false)
    if (Path.Exists(file_path))
    {
        new CylinderPlayer(grid).LoadImage(file_path).Play();
        return;
    }
    grid.DrawImage().SaveImage("CylinderGrid");
    
    var rand = Utilities.CreateRandomWithPrintedSeed();
    var startCell = grid.GetAllCells().RandomItem(rand);
    var endCell = grid.GetAllCells().RandomItem(rand);
    var distance_value = new DistanceMap<GridCell>(grid, startCell);
    var path_value = distance_value.GetPathMap(endCell);
    
    var image = grid.DrawImage(distance_value.GetCellColorByDistanceValue());
    image.Mutate(ctx => ctx.Rotate(RotateMode.Rotate90).Flip(FlipMode.Vertical));
    image.SaveImage(file_name);
}

void TestMobius()
{
    var grid = new MobiusPlane(32, 32);
    KruskalLink(grid);
    var file_name = "MobiusGridDistance";
    var file_path = file_name + ".png";
    // var file_path = @"C:\Work\Projects\PCGBook\PCG.Maze\bin\Debug\net7.0\01-uv-texture.png";
    // if (Path.Exists(file_path))
    // {
    //     new CylinderPlayer(grid).LoadImage(file_path).Play();
    //     return;
    // }
    grid.DrawImage().SaveImage("MobiusGrid");
    
    var rand = Utilities.CreateRandomWithPrintedSeed();
    var startCell = grid.GetAllCells().RandomItem(rand);
    var endCell = grid.GetAllCells().RandomItem(rand);
    var distance_value = new DistanceMap<GridCell>(grid, startCell);
    var path_value = distance_value.GetPathMap(endCell);
    
    var image = grid.DrawImage(distance_value.GetCellColorByDistanceValue());
    image.SaveImage(file_name);
}

// TestGridAndDistanceMap();
// TestCircle();
// DeadEndProgram();
// DeadEndRemovalProgram();
// TestWeaveGrid();
// TestHexGrid();
// MaskProgram();
// TestNormalGrid();

// TestRayLib();
// TestCylinder();
TestMobius();