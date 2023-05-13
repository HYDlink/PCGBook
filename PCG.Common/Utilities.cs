using System.Diagnostics;
using SixLabors.ImageSharp.Formats.Png;

namespace PCG.Common;

public static class Utilities
{
    public static void SaveImage<TPixel>(this Image<TPixel> outputImg, string name = "corner") where TPixel : unmanaged, IPixel<TPixel>
    {
        var output_corner_x4_png = $"{name}.png";
        outputImg.Save(output_corner_x4_png, new PngEncoder());
        Process.Start(new ProcessStartInfo(output_corner_x4_png) { UseShellExecute = true });
    }
}