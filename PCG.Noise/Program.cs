// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using PCG.Noise;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using static PCG.Noise.BilinearFilterClass;
using static PCG.Noise.DiamondFractalClass;

Console.WriteLine("Hello, World!");
const int WIDTH = 32;
const int HEIGHT = 32;

var seed = new Random().Next();
Console.WriteLine($"Random Seed: {seed}");
var random = new Random(seed);

var value_noise = ByteImage.Random(random, WIDTH, HEIGHT);

var value_noise_img = value_noise.ValueNoiseImg();

// bilinear filter
var bilinear_noise_img = BilinearEnlarge4(value_noise).ValueNoiseImg();

SaveImage(value_noise_img, "valueNoise");
SaveImage(bilinear_noise_img, "bilinearNoise");
var clone = value_noise_img.Clone();
clone.Mutate(c => c.Resize(new Size(WIDTH * 4, HEIGHT * 4), new BicubicResampler(), false));
SaveImage(clone, "ImageSharpBiCubic");


var diamond_filter = DiamondSteps(value_noise, random, 8);
// DiamondFilter(DiamondFilter(DiamondFilter(value_noise)));
var diamond_img = diamond_filter.ValueNoiseImg();
SaveImage(diamond_img, "DiamondFilter");

T Recursive<T>(T value, Func<T, T> func, int times)
{
    var tmp = value;
    for (int i = 0; i < times; i++) tmp = func(tmp);

    return tmp;
}


void SaveImage<TPixel>(Image<TPixel> outputImg, string name = "corner") where TPixel : unmanaged, IPixel<TPixel>
{
    var output_corner_x4_png = $"{name}.png";
    outputImg.Save(output_corner_x4_png, new PngEncoder());
    Process.Start(new ProcessStartInfo(output_corner_x4_png) { UseShellExecute = true });
}