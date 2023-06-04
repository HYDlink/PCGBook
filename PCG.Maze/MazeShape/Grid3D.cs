using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace PCG.Maze.MazeShape;

using static Raylib;
using static Raylib_cs.Color;

public class Grid3D : IMazeMap<Grid3DCell>
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }

    public Grid3DCell?[,,] Cells { get; set; }
    public virtual IEnumerable<Grid3DCell> GetAllCells() => Cells.OfType<Grid3DCell>().ToList();

    public Grid3D(int width, int height, int length)
    {
        Width = width;
        Height = height;
        Length = length;

        Debug.Assert(Width >= 2 && Height >= 2);

        InitCells();

        // 设置 Cell 的邻居方向
        SetAllCellsNeighbor();
    }

    protected virtual void InitCells()
    {
        Cells = new Grid3DCell[Length, Height, Width];
        for (var z = 0; z < Length; z++)
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            Cells[z, y, x] = InitCell(x, y, z);
    }


    public virtual Grid3DCell InitCell(int x, int y, int z) => new Grid3DCell(x, y, z);

    protected virtual void SetAllCellsNeighbor()
    {
        for (var z = 0; z < Length; z++)
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var cur_cell = Cells[z, y, x];
            if (cur_cell is null)
                continue;
            if (x + 1 < Width)
            {
                var right_cell = Cells[z, y, x + 1];
                if (right_cell != null)
                {
                    cur_cell.Right = right_cell;
                    right_cell.Left = cur_cell;
                }
            }

            if (y + 1 < Height)
            {
                var down_cell = Cells[z, y + 1, x];
                if (down_cell != null)
                {
                    cur_cell.Down = down_cell;
                    down_cell.Up = cur_cell;
                }
            }

            if (z + 1 < Length)
            {
                var forward_cell = Cells[z + 1, y, x];
                if (forward_cell != null)
                {
                    cur_cell.Forward = forward_cell;
                    forward_cell.Backward = cur_cell;
                }
            }
        }
    }

    public Grid3DCell GetCell(int x, int y, int z) => Cells[z, y, x];

    public void Print()
    {
        throw new NotImplementedException();
    }

    public Image<Rgba32> DrawImage(Func<Grid3DCell, Rgba32>? cellColorGetter = null)
    {
        throw new NotImplementedException();
    }


    public void DrawRayLib()
    {
        // Initialization
        //--------------------------------------------------------------------------------------
        const int screenWidth = 1080;
        const int screenHeight = 720;
        var cubeSize = new Vector3(1, 1, 1);

        InitWindow(screenWidth, screenHeight, "raylib [core] example - 3d camera free");

        var cameraSize = 4f * Width;
        var camera = new Camera3D(new Vector3(cameraSize, 0, cameraSize),
            new Vector3(0, 0, 0), Vector3.UnitY, 45,
            CameraProjection.CAMERA_PERSPECTIVE);
        DisableCursor(); // Limit cursor to relative movement inside the window

        SetTargetFPS(60); // Set our game to run at 60 frames-per-second
        //--------------------------------------------------------------------------------------

        // TODO update camera, rotate around Map Center

        var cameraXAngle = 0f;
        var cameraYAngle = 0f;
        var deltaAngle = 0.1f;

        
        
        // SetMaterialTexture();
        // Main game loop
        while (!WindowShouldClose()) // Detect window close button or ESC key
        {
            // Update
            //----------------------------------------------------------------------------------
            // UpdateCamera(ref camera, CameraMode.CAMERA_CUSTOM);

            if (IsKeyDown(KeyboardKey.KEY_A))
            {
                cameraXAngle -= deltaAngle;
            }

            if (IsKeyDown(KeyboardKey.KEY_D))
            {
                cameraXAngle += deltaAngle;
            }

            if (IsKeyDown(KeyboardKey.KEY_W))
            {
                cameraYAngle += deltaAngle;
            }

            if (IsKeyDown(KeyboardKey.KEY_S))
            {
                cameraYAngle -= deltaAngle;
            }

            var cy = MathF.Sin(cameraYAngle) * cameraSize;
            var cr = MathF.Cos(cameraYAngle) * cameraSize;
            var cx = cr * MathF.Sin(cameraXAngle);
            var cz = cr * MathF.Cos(cameraXAngle);

            camera.position = new Vector3(cx, cy, cz);

            if (IsKeyDown(KeyboardKey.KEY_Z))
                camera.target = new Vector3
                (
                    0.0f, 0.0f, 0.0f
                );
            ;
            //----------------------------------------------------------------------------------

            // Draw
            //----------------------------------------------------------------------------------
            BeginDrawing();

            ClearBackground(RAYWHITE);

            BeginMode3D(camera);

            Vector3 GetCellCenterPos(int x, int y, int z)
            {
                var pos = (new Vector3(x - Width / 2, y - Height / 2, z - Height / 2) * 2 + Vector3.One) * cubeSize;
                return pos;
            }

            void DrawCorridor(Vector3 start, Vector3 end)
                => DrawCapsule(start, end, 0.2f, 1, 2, SKYBLUE);


            for (var z = 0; z < Length; z++)
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var cell = Cells[z, y, x];
                if (cell is null) continue;
                var pos = GetCellCenterPos(x, y, z);

                // TODO draw corridors
                if (cell is { HasLinkRight: true })
                {
                    var right_pos = GetCellCenterPos(x + 1, y, z);
                    DrawCorridor(pos, right_pos);
                }

                if (cell is { HasLinkDown: true })
                {
                    var neighbor_pos = GetCellCenterPos(x, y + 1, z);
                    DrawCorridor(pos, neighbor_pos);
                }

                if (cell is { HasLinkForward: true })
                {
                    var neighbor_pos = GetCellCenterPos(x, y, z + 1);
                    DrawCorridor(pos, neighbor_pos);
                }

                DrawCube(pos, 1.0f, 1.0f, 1.0f, RED);
                DrawCubeWires(pos, 1.0f, 1.0f, 1.0f, MAROON);
            }

            DrawGrid(10, 1.0f);

            EndMode3D();

            DrawRectangle(10, 10, 320, 133, Fade(SKYBLUE, 0.5f));
            DrawRectangleLines(10, 10, 320, 133, BLUE);

            DrawText("Free camera default controls:", 20, 20, 10, BLACK);
            DrawText("- Mouse Wheel to Zoom in-out", 40, 40, 10, DARKGRAY);
            DrawText("- Mouse Wheel Pressed to Pan", 40, 60, 10, DARKGRAY);
            DrawText("- Alt + Mouse Wheel Pressed to Rotate", 40, 80, 10, DARKGRAY);
            DrawText("- Alt + Ctrl + Mouse Wheel Pressed for Smooth Zoom", 40, 100, 10, DARKGRAY);
            DrawText("- Z to zoom to (0, 0, 0)", 40, 120, 10, DARKGRAY);

            EndDrawing();
            //----------------------------------------------------------------------------------
        }

        // De-Initialization
        //--------------------------------------------------------------------------------------
        CloseWindow(); // Close window and OpenGL context
        //--------------------------------------------------------------------------------------
    }

    public void RemoveCell(Grid3DCell cell)
    {
    }
}

public record Grid3DCell(int X, int Y, int Z) : CellBase
{
    public Grid3DCell? Left { get; set; }
    public Grid3DCell? Right { get; set; }
    public Grid3DCell? Up { get; set; }
    public Grid3DCell? Down { get; set; }
    public Grid3DCell? Forward { get; set; }
    public Grid3DCell? Backward { get; set; }

    public bool HasLeft => Left != null;
    public bool HasRight => Right != null;
    public bool HasUp => Up != null;
    public bool HasDown => Down != null;
    public bool HasForward => Forward != null;
    public bool HasBackward => Backward != null;

    public bool HasLinkLeft => HasLeft && Links.Contains(Left);
    public bool HasLinkRight => HasRight && Links.Contains(Right);
    public bool HasLinkUp => HasUp && Links.Contains(Up);
    public bool HasLinkDown => HasDown && Links.Contains(Down);
    public bool HasLinkForward => HasForward && Links.Contains(Down);
    public bool HasLinkBackward => HasBackward && Links.Contains(Down);

    public override IEnumerable<Grid3DCell> GetNeighbors() => GetNeighborsOnGridCell();

    public IEnumerable<Grid3DCell> GetNeighborsOnGridCell()
    {
#pragma warning disable CS8603
        if (HasLeft)
            yield return Left;
        if (HasRight)
            yield return Right;
        if (HasUp)
            yield return Up;
        if (HasDown)
            yield return Down;
        if (HasForward)
            yield return Forward;
        if (HasBackward)
            yield return Backward;
#pragma warning restore CS8603
    }

    public override IEnumerable<Grid3DCell> GetLinks() => Links.OfType<Grid3DCell>();

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override string ToString() => $"({X}, {Y}, {Z})";
}