// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using PCG.Noise;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using static PCG.Noise.BilinearFilterClass;
using static PCG.Noise.DiamondFractalClass;
using static PCG.Common.Utilities;

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

value_noise_img.SaveImage("valueNoise");
bilinear_noise_img.SaveImage("bilinearNoise");
var clone = value_noise_img.Clone();
clone.Mutate(c => c.Resize(new Size(WIDTH * 4, HEIGHT * 4), new BicubicResampler(), false));
clone.SaveImage("ImageSharpBiCubic");


var diamond_filter = DiamondSteps(value_noise, random, 8);
// DiamondFilter(DiamondFilter(DiamondFilter(value_noise)));
var diamond_img = diamond_filter.ValueNoiseImg();
diamond_img.SaveImage("DiamondFilter");

T Recursive<T>(T value, Func<T, T> func, int times)
{
    var tmp = value;
    for (int i = 0; i < times; i++) tmp = func(tmp);

    return tmp;
}