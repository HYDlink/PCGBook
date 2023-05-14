using System.Diagnostics;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;

namespace PCG.Common;

public static class Utilities
{
    public static void SaveImage<TPixel>(this Image<TPixel> outputImg, string name = "corner")
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var output_corner_x4_png = $"{name}.png";
        outputImg.Save(output_corner_x4_png, new PngEncoder());
        Process.Start(new ProcessStartInfo(output_corner_x4_png) { UseShellExecute = true });
    }

    public static Random CreateRandomWithPrintedSeed(int seed = -1)
    {
        if (seed == -1) seed = new Random().Next();
        Console.WriteLine($"Random seed: {seed}");
        return new Random(seed);
    }

    public static void EncodeGif(this IEnumerable<Image<Rgba32>> images, int frameDelay = 10)
    {
        if (!images.Any()) return;

        // Image dimensions of the gif.
        var width = images.First().Width;
        var height = images.First().Height;

        // Delay between frames in (1/100) of a second.
        
        // Create empty image.
        using Image<Rgba32> gif = new(width, height);

        // Set animation loop repeat count to 5.
        // var gifMetaData = gif.Metadata.GetGifMetadata();
        // gifMetaData.RepeatCount = 5;
        
        // Set the delay until the next image is displayed.
        GifFrameMetadata metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();
        metadata.FrameDelay = frameDelay;

        foreach (var image in images)
        {
            // Set the delay until the next image is displayed.
            metadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
            metadata.FrameDelay = frameDelay;

            // Add the color image to the gif.
            gif.Frames.AddFrame(image.Frames.RootFrame);
        }

        // Save the final result.
        var output_filename = "output.gif";
        gif.SaveAsGif(output_filename);
        Process.Start(new ProcessStartInfo(output_filename) { UseShellExecute = true });
    }
    
    public static void Shuffle<T>(this IList<T> list, Random rng)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static T RandomItem<T>(this IEnumerable<T> items, Random random)
    {
        if (items.Any()) return items.ElementAt(random.Next(items.Count()));
        else return default;
    }
}