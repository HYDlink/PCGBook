using System.Diagnostics;
using PCG.Common;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace PCG.Maze.MazeShape;

public record MaskGrid(int Width, int Height) : Map2D(Width, Height)
{
    public const int InaccesableValue = -1;

    public static MaskGrid FromString(string asciiMap)
    {
        var lines = asciiMap.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var height = lines.Length;
        Debug.Assert(height > 0, "input asciiMap has no text");
        var is_same_width = lines.DistinctBy(l => l.Length).Count() == 1;
        Debug.Assert(is_same_width, "input asciiMap is not all same width");
        var width = lines[0].Length;

        var mask_grid = new MaskGrid(width, height);

        var y = 0;
        foreach (var line in lines)
        {
            var x = 0;
            foreach (var ch in line)
            {
                mask_grid[y, x] = ch == 'X' ? InaccesableValue : ch;
                x++;
            }

            y++;
        }

        return mask_grid;
    }

    public static MaskGrid FromImage(string imageFilePath) => FromImage(Image.Load<Rgba32>(imageFilePath));

    public static MaskGrid FromImage(Image<Rgba32> image)
    {
        var mask_grid = new MaskGrid(image.Width, image.Height);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var p = image[x, y];
                int value = p.R << 24 | p.G << 16 | p.B << 8;

                var cannot_access = value == 0;
                mask_grid[y, x] = cannot_access ? -1 : value;
            }
        }

        return mask_grid;
    }
    
    public static Image<Rgba32> DrawTextToImage(string text, string fontPath = @"C:\Windows\Fonts\MComputer PRC Bold.ttf", int fontSize = 16)
    {
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var text_height = lines.Count();
        var text_width = lines.MaxBy(t => t.Count()).Count();

        FontCollection collection = new();
        FontFamily family = collection.Add(fontPath);
        Font font = family.CreateFont(fontSize, FontStyle.Bold);
        
        var image = new Image<Rgba32>(text_width * fontSize, text_height * fontSize);
        image.Mutate(ctx =>
        {
            ctx.DrawText(text, font, Color.White, PointF.Empty);
        });
        
        image.SaveImage("Text");
        return image;
    }
}