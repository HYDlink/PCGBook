namespace PCG.Dungeon;

public abstract class Represent
{
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public void NewMap(int width, int height)
    {
        Width = width;
        Height = height;
        NewMapInternal(width, height);
    }

    protected abstract void NewMapInternal(int width, int height);

    public void DrawPixel(int x, int y)
    {
        ValidateX(x);
        ValidateY(y);
        DrawPixelInternal(x, y);
    }

    protected abstract void DrawPixelInternal(int x, int y);

    public void DrawRectangle(int x, int y, int w, int h)
    {
        // 假设 Width = 1，那么画一个像素点等同于画一个 (0,0,1,1) 的 Rectangle，这个时候，应该保证 X 与 Width 的计算和 < 1，也就是 x + w < Width
        var valid = ValidateX(x) && ValidateX(x + w - 1)
                                 && ValidateY(y) && ValidateY(y + h - 1);
        DrawRectangleInternal(x, y, w, h);
    }

    protected abstract void DrawRectangleInternal(int x, int y, int w, int h);

    public void DrawLine(int x1, int y1, int x2, int y2)
    {
        var valid = ValidateX(x1) && ValidateX(x2)
                                  && ValidateY(y1) && ValidateY(y2);
        DrawLineInternal(x1, y1, x2, y2);
    }

    public virtual void DrawLineInternal(int x1, int y1, int x2, int y2)
    {
        DrawLineBresnham(x1, y1, x2, y2, DrawPixelInternal);
    }

    public static void DrawLineBresnham(int x0, int y0, int x1, int y1, Action<int, int> pixelDrawer)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2; /* error value e_xy */

        for (;;)
        {
            /* loop */
            pixelDrawer?.Invoke(x0, y0);

            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            } /* e_xy+e_x > 0 */

            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            } /* e_xy+e_y < 0 */
        }
    }

    public abstract void ClearMap();

    public virtual void Show()
    {
    }

    public bool IsValidX(int x) => x >= 0 && x < Width;
    public bool IsValidY(int y) => y >= 0 && y < Height;

    public bool ValidateX(int x) =>
        IsValidX(x) ? true : throw new IndexOutOfRangeException($"X out of range, X: {x}, Width: {Width}");

    public bool ValidateY(int y) =>
        IsValidY(y) ? true : throw new IndexOutOfRangeException($"Y out of range, Y: {y}, Height: {Height}");
}