namespace PCG.Noise;

public class BilinearFilterClass
{
    public static ByteImage BilinearEnlarge4(ByteImage source)
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
}