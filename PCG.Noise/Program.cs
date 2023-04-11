// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Numerics;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing.Processors.Transforms;


Console.WriteLine("Hello, World!");
const int WIDTH = 64;
const int HEIGHT = 64;

var seed = new Random().Next();
Console.WriteLine($"Random Seed: {seed}");
var random = new Random(seed);


var value_noise = ByteImage.Random(random, WIDTH, HEIGHT);


Image<Rgba32> ValueNoiseImg(ByteImage valueNoise)
{
    var (width, height) = valueNoise;
    var img = new Image<Rgba32>(width, height);
    for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
    {
        var packed = valueNoise[y, x];
        img[y, x] = new Rgba32(packed, packed, packed);
    }

    return img;
}


var value_noise_img = ValueNoiseImg(value_noise);


// bilinear filter
var bilinear_noise_img = ValueNoiseImg(BilinearImage(value_noise));

ByteImage BilinearImage(ByteImage source)
{
    var (width, height) = source;
    var bilinear_image = new ByteImage(width * 4, height * 4);
    for (int y = 0; y < height * 4; y++)
    for (int x = 0; x < width * 4; x++)
    {
        var left = x / 4;
        var right = left < width - 1 ? left + 1 : left;

        var up = y / 4;
        var bottom = up < height - 1 ? up + 1 : up;

        var upper = left >= width - 1
            ? source[up, left]
            : (byte)((source[up, left] * (4 - x % 4) + source[up, right] * (x % 4)) / 4);
        var lower = left >= width - 1
            ? source[bottom, left]
            : (byte)((source[bottom, left] * (4 - x % 4) + source[bottom, right] * (x % 4)) /
                     4);
        var value = up >= height - 1 ? upper : (byte)((upper * (4 - y % 4) + lower * (y % 4)) / 4);
        bilinear_image[y, x] = value;
    }

    return bilinear_image;
}
// valueNoiseImg[y, x] = random.RandomColor();
// valueNoiseImg[y, x] = new Rgba32(random.RandomVector3());


// Diamond Filter

// 这两个实现都会分配数组作为内存，会产生很大量的 SOH，生成一个 4033 的 Diamond 图花费了 620M 的 SOH
// 因为 params byte[] 本身创建了数组
// byte AverageByte(params byte[] bytes) => (byte)bytes.Average(b => b);
// byte AverageByte(params byte[] bytes)
// {
//     int accumulate = 0;
//     foreach (var b in bytes)
//         accumulate += b;
//
//     return (byte)(accumulate / bytes.Length);
// }

// 在 Top-Level Statements 中，所有定义的函数都是 local function，不允许重载
// 改成 除 4 或者更高，会有奇妙的 Diamond 效果www，这才是真正的 Diamond Square！
byte AverageByte3(byte b1, byte b2, byte b3) => (byte)((b1 + b2 + b3) / 3);
byte AverageByte(byte b1, byte b2, byte b3, byte b4) => (byte)((b1 + b2 + b3 + b4) / 4);

ByteImage DiamondFilter(ByteImage byteImage)
{
    var (width, height) = byteImage;
    var diamond = new ByteImage(byteImage.Width * 2 - 1, byteImage.Height * 2 - 1);
    for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
        diamond[y * 2, x * 2] = byteImage[y, x];

    // 4 dir to center
    for (int y = 0; y < height - 1; y++)
    for (int x = 0; x < width - 1; x++)
    {
        var value =
            AverageByte(byteImage[y, x], byteImage[y + 1, x], byteImage[y, x + 1], byteImage[y + 1, x + 1]);
        if (value == 0)
            Debugger.Break();
        diamond[y * 2 + 1, x * 2 + 1] = value;
    }

    // Square step - h_line center, v_line center
    for (int y = 0; y < height - 1; y++)
    for (int x = 0; x < width - 1; x++)
    {
        var (ul, ur, center, bl, br) =
            (byteImage[y, x], byteImage[y, x + 1], diamond[y * 2 + 1, x * 2 + 1], byteImage[y + 1, x],
                byteImage[y + 1, x + 1]);
        var up_center = AverageByte3(ul, ur, center);
        var left_center = AverageByte3(ul, bl, center);
        var right_center = AverageByte3(br, ur, center);
        var bottom_center = AverageByte3(bl, br, center);
        diamond[y * 2, x * 2 + 1] = up_center;
        diamond[y * 2 + 2, x * 2 + 1] = bottom_center;
        diamond[y * 2 + 1, x * 2] = left_center;
        diamond[y * 2 + 1, x * 2 + 2] = right_center;
    }

    return diamond;
}

T Recursive<T>(T value, Func<T, T> func, int times)
{
    var tmp = value;
    for (int i = 0; i < times; i++) tmp = func(tmp);

    return tmp;
}

SaveImage(value_noise_img, "valueNoise");
SaveImage(bilinear_noise_img, "bilinearNoise");
var clone = value_noise_img.Clone();
clone.Mutate(c => c.Resize(new Size(WIDTH * 4, HEIGHT * 4), new BicubicResampler(), false));
SaveImage(clone, "ImageSharpBiCubic");


var diamond_filter = Recursive(value_noise, DiamondFilter, 3);
    // DiamondFilter(DiamondFilter(DiamondFilter(value_noise)));
var diamond_img = ValueNoiseImg(diamond_filter);
SaveImage(diamond_img, "DiamondFilter");


void SaveImage<TPixel>(Image<TPixel> outputImg, string name = "corner") where TPixel : unmanaged, IPixel<TPixel>
{
    var output_corner_x4_png = $"{name}.png";
    outputImg.Save(output_corner_x4_png, new PngEncoder());
    Process.Start(new ProcessStartInfo(output_corner_x4_png) { UseShellExecute = true });
}

Rgba32 FromByte(byte b) => new Rgba32(b, b, b);

public static class RandomExtension
{
    public static Vector3 RandomVector3(this Random random)
        => new((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

    public static Rgba32 RandomColor(this Random random)
        => new((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

    public static Rgba32 RandomGray(this Random random)
    {
        var gray = (byte)random.Next(256);
        return new Rgba32(gray, gray, gray);
    }
}

public record ByteImage(int Width, int Height)
{
    private byte[] Values { get; set; } = new byte[Width * Height];

    public byte this[int y, int x]
    {
        get => Values[Width * y + x];
        set => Values[Width * y + x] = value;
    }

    public static ByteImage Random(Random random, int width, int height)
    {
        var byte_image = new ByteImage(width, height);
        random.NextBytes(byte_image.Values);
        return byte_image;
    }
}