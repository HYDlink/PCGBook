// See https://aka.ms/new-console-template for more information

using PCG.Common;
using PCG.Maze;
using PCG.Maze.MazeShape;
using PCG.Maze.ValueMap;
using static PCG.Maze.MazeShape.MaskGrid;

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
        new MazeGenerator(grid).BackTrackLink();
        grid.DrawImage().SaveImage("MaskedMaze");
        // var distance_value = DistanceMap.GetDistanceMap(grid, grid.GetAllCells().First());
        // grid.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("MaskedMaze");
    }
}

void DeadEndProgram()
{
    var grid = new Grid(8, 8);
    var generator = new MazeGenerator(grid);

    // RecordGif(generator);
    generator.BackTrackLink();
    var map = DeadEndMap.GetDeadEndMap(grid);
    grid.DrawImage().SaveImage("Maze");
}

void RecordGif(MazeGenerator generator)
{
    var images = new List<Image<Rgba32>>();
    generator.BackTrackLink(StepDrawImage);

    void StepDrawImage(Grid grid)
    {
        var draw_image = grid.DrawImage();
        images.Add(draw_image);
    }

    images.EncodeGif();
}

var circle = new Circle(6, 20);

void BackTrackLink(Circle maze, Action<Grid>? onStepFinish = null)
{
    var random = Utilities.CreateRandomWithPrintedSeed();

    var unvisited = maze.GetAllCells().ToList();

    void DFS(CircleCell cell)
    {
        unvisited.Remove(cell);
        var neighbors = cell.GetNeighbors().ToList();
        Utilities.Shuffle(neighbors, random);
        foreach (var neighbor in neighbors)
        {
            if (unvisited.Contains(neighbor))
            {
                cell.Link(neighbor, true);
                DFS(neighbor);
            }
        }
    }

    while (unvisited.Any())
        DFS(unvisited.First());
}

// BackTrackLink(circle);
circle.DrawImage().SaveImage("CircleMaze");


// grid.Print();
// var distance_value = DistanceMap.GetDistanceMap(grid, 16, 16);
// grid.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("Maze");