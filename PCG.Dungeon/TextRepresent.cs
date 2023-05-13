using System.Runtime.CompilerServices;
using System.Text;

namespace PCG.Dungeon;

public class TextRepresent : Represent
{
    private const char DRAWED = 'X';
    private const char H_LINE = '─';
    private char[,] map;

    protected override void NewMapInternal(int width, int height)
    {
        map = new char[height, width];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void DrawPixelInternal(int x, int y)
    {
        map[y, x] = DRAWED;
    }

    protected override void DrawRectangleInternal(int x, int y, int w, int h)
    {
        map[y, x] = '/';
        map[y, x + w - 1] = '\\';
        map[y + h - 1, x] = '/';
        map[y + h - 1, x + w - 1] = '\\';

        for (int cx = x + 1; cx < x + w - 1; cx++)
        {
            map[y, cx] = H_LINE;
            map[y + h - 1, cx] = H_LINE;
        }

        for (int cy = y; cy < y + h; cy++)
        {
            map[cy, x] = '|';
            map[cy, x + w - 1] = '|';
        }
    }

    public override void DrawLineInternal(int x1, int y1, int x2, int y2)
    {
        DrawLineBresnham(x1, y1, x2, y2, (x, y) => map[y, x] = 'o');
    }

    public override void ClearMap()
    {
        NewMapInternal(Width, Height);
    }

    public override void Show()
    {
        var sb = new StringBuilder((Width + 2) * Height);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                sb.Append(map[y, x]);
            }

            sb.AppendLine();
        }

        var str = sb.ToString();
        Console.Write(str);
    }
}