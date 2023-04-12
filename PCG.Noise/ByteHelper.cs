namespace PCG.Noise;

public static class ByteHelper
{
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
    public static byte AverageByte(byte b1, byte b2, byte b3) => (byte)((b1 + b2 + b3) / 3);
    public static byte AverageByte(byte b1, byte b2, byte b3, byte b4) => (byte)((b1 + b2 + b3 + b4) / 4);
    public static byte ClampToByte(int value) => (byte)Math.Clamp(value, byte.MinValue, byte.MaxValue);
}