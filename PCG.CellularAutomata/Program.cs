// See https://aka.ms/new-console-template for more information


using System.Reflection;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

Console.WriteLine("Hello, World!");
var cave_ca = new CaveCA(64, 64, 0.5f, 5);
cave_ca.Execute();
ShowCaveCA(cave_ca);

void ShowCaveCA(CaveCA cave)
{
    var map = cave.Map;
// Create a new image with the same dimensions as the boolean array
    int width = map.GetLength(0);
    int height = map.GetLength(1);
    var image = new Image<Rgb24>(width, height);

// Loop through each cell in the boolean array
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            image[x, y] = map[x, y] switch
            {
                CaveCell.Empty => Color.Gray,
                CaveCell.Stone => Color.Black,
                CaveCell.Wall => Color.Red,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

// Save the image to a file
    image.Save("grid.png");
}