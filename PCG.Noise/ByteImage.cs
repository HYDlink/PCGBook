namespace PCG.Noise;

public record ByteImage(int Width, int Height)
{
    private byte[] Values { get; set; } = new byte[Width * Height];

    public byte this[int y, int x]
    {
        get => Values[Width * y + x];
        set => Values[Width * y + x] = value;
    }
    
    public Image<Rgba32> ValueNoiseImg()
    {
        Rgba32 FromByte(byte b) => new(b, b, b);
        
        var img = new Image<Rgba32>(Width, Height);
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var packed = this[y, x];
            img[y, x] = FromByte(packed);
        }

        return img;
    }

    public static ByteImage Random(Random random, int width, int height)
    {
        var byte_image = new ByteImage(width, height);
        random.NextBytes(byte_image.Values);
        return byte_image;
    }
}