// See https://aka.ms/new-console-template for more information

using PCG.Common;
using PCG.Maze;

var grid = new Grid(32, 32);
var generator = new MazeGenerator(grid);
generator.WilsonLink();
// grid.Print();
var distance_value = DistanceMap.GetDistanceMap(grid, 16, 16);
grid.DrawImage(distance_value.GetCellColorByDistanceValue()).SaveImage("Maze");