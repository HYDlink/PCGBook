using System.Diagnostics;

namespace PCG.Noise;

public static class DiamondFractalClass
{
    public static ByteImage DiamondFractal(ByteImage byteImage, Random random, int randOffset)
    {
        byte AppendRandOffset(byte value)
            => ByteHelper.ClampToByte(value + random.Next(-randOffset, randOffset));

        var (width, height) = byteImage;
        var diamond = new ByteImage(byteImage.Width * 2 - 1, byteImage.Height * 2 - 1);
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            diamond[y * 2, x * 2] = byteImage[y, x];

        // Diamond step - 4 dir to center
        for (int y = 0; y < height - 1; y++)
        for (int x = 0; x < width - 1; x++)
        {
            var value =
                ByteHelper.AverageByte(byteImage[y, x], byteImage[y + 1, x], byteImage[y, x + 1], byteImage[y + 1, x + 1]);
            if (value == 0)
                Debugger.Break();
            diamond[y * 2 + 1, x * 2 + 1] = AppendRandOffset(value);
        }

        // Square step - h_line center, v_line center
        for (int y = 0; y < height - 1; y++)
        for (int x = 0; x < width - 1; x++)
        {
            var (ul, ur, center, bl, br) =
                (byteImage[y, x], byteImage[y, x + 1], diamond[y * 2 + 1, x * 2 + 1], byteImage[y + 1, x],
                    byteImage[y + 1, x + 1]);
            var up_center = ByteHelper.AverageByte(ul, ur, center);
            var left_center = ByteHelper.AverageByte(ul, bl, center);
            var right_center = ByteHelper.AverageByte(br, ur, center);
            var bottom_center = ByteHelper.AverageByte(bl, br, center);
            diamond[y * 2, x * 2 + 1] = AppendRandOffset(up_center);
            diamond[y * 2 + 2, x * 2 + 1] = AppendRandOffset(bottom_center);
            diamond[y * 2 + 1, x * 2] = AppendRandOffset(left_center);
            diamond[y * 2 + 1, x * 2 + 2] = AppendRandOffset(right_center);
        }

        return diamond;
    }

    public static ByteImage DiamondSteps(ByteImage src, Random random, int step)
    {
        var accumulate = src;
        var randOffset = 32;
        for (int i = 0; i < step; i++)
        {
            accumulate = DiamondFractal(accumulate, random, randOffset);
            randOffset /= 2;
        }

        return accumulate;
    }
}