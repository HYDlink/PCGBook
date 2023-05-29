using System.Numerics;
using Raylib_cs;

namespace PCG.Maze.MazeShape;

using static Raylib;
using static Raylib_cs.Color;

public class Grid3D : IMazeMap<Grid3DCell>
{
    public IEnumerable<Grid3DCell> GetAllCells()
    {
        throw new NotImplementedException();
    }

    public void Print()
    {
        throw new NotImplementedException();
    }

    public Image<Rgba32> DrawImage(Func<Grid3DCell, Rgba32>? cellColorGetter = null)
    {
        // Initialization
        //--------------------------------------------------------------------------------------
        const int screenWidth = 800;
        const int screenHeight = 450;

        InitWindow(screenWidth, screenHeight, "raylib [core] example - 3d camera free");

        var camera = new Camera3D(new Vector3(10, 10, 10),
            new Vector3(0, 0, 0), Vector3.UnitY, 45,
            CameraProjection.CAMERA_PERSPECTIVE);
        var cubePosition = new Vector3(0, 0, 0);
        DisableCursor(); // Limit cursor to relative movement inside the window

        SetTargetFPS(60); // Set our game to run at 60 frames-per-second
        //--------------------------------------------------------------------------------------

        // Main game loop
        while (!WindowShouldClose()) // Detect window close button or ESC key
        {
            // Update
            //----------------------------------------------------------------------------------
            UpdateCamera(ref camera, CameraMode.CAMERA_FREE);

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

            DrawCube(cubePosition, 2.0f, 2.0f, 2.0f, RED);
            DrawCubeWires(cubePosition, 2.0f, 2.0f, 2.0f, MAROON);

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

        return null;
    }

    public void RemoveCell(Grid3DCell cell)
    {
    }
}

public record Grid3DCell : CellBase
{
    public override IEnumerable<CellBase> GetNeighbors()
    {
        throw new NotImplementedException();
    }
}