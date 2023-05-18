// See https://aka.ms/new-console-template for more information

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

void TestGridAndDistanceMap()
{
    var (width, height) = (32, 32);
    var grid = new Grid(width, height);
    // grid.DrawImageWithInset().SaveImage("Maze");
    // return;
    
    KruskalLink(grid);
    // RecordGif(grid, KruskalLink);
    // return;
    // grid.RemoveDeadEndPath(0.5f);

    var rand = Utilities.CreateRandomWithPrintedSeed();
    var startCell = grid.GetAllCells().RandomItem(rand);
    var endCell = grid.GetAllCells().RandomItem(rand);
    var distance_value = new DistanceMap<GridCell>(grid, startCell);
    var path_value = distance_value.GetPathMap(endCell);
    grid.DrawImageWithInset().SaveImage("GridMaze");
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

TestGridAndDistanceMap();
// TestCircle();
// DeadEndProgram();
// DeadEndRemovalProgram();