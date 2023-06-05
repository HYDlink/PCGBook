using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Color;
using Color = Raylib_cs.Color;

namespace PCG.Maze;

public class RaylibPlayer
{
    public float CameraSize = 10f;
    protected Camera3D camera;

    public virtual void Init()
    {
    }

    public virtual void Update()
    {
    }

    public virtual void Draw()
    {
    }

    protected virtual int screenWidth => 1080;
    protected virtual int screenHeight => 720;
    protected virtual string Title => "Raylib Example";

    public void Play()
    {
        // Initialization
        //--------------------------------------------------------------------------------------

        InitWindow(screenWidth, screenHeight, Title);

        camera = new Camera3D(new Vector3(CameraSize, 0, CameraSize),
            new Vector3(0, 0, 0), Vector3.UnitY, 45,
            CameraProjection.CAMERA_PERSPECTIVE);
        DisableCursor(); // Limit cursor to relative movement inside the window

        SetTargetFPS(60); // Set our game to run at 60 frames-per-second
        //--------------------------------------------------------------------------------------

        // TODO update camera, rotate around Map Center

        var cameraXAngle = 0f;
        var cameraYAngle = 0f;
        var deltaAngle = 0.1f;


        Init();

        // SetMaterialTexture();
        // Main game loop
        while (!WindowShouldClose()) // Detect window close button or ESC key
        {
            // Update
            //----------------------------------------------------------------------------------
            void UpdateCamera()
            {
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

                var cy = MathF.Sin(cameraYAngle) * CameraSize;
                var cr = MathF.Cos(cameraYAngle) * CameraSize;
                var cx = cr * MathF.Sin(cameraXAngle);
                var cz = cr * MathF.Cos(cameraXAngle);

                camera.position = new Vector3(cx, cy, cz);

                if (IsKeyDown(KeyboardKey.KEY_Z))
                    camera.target = new Vector3
                    (
                        0.0f, 0.0f, 0.0f
                    );
            }

            UpdateCamera();

            //----------------------------------------------------------------------------------

            // Draw
            //----------------------------------------------------------------------------------
            BeginDrawing();

            ClearBackground(RAYWHITE);

            BeginMode3D(camera);

            Draw();

            EndMode3D();

            void DrawHint()
            {
                DrawRectangle(10, 10, 320, 133, Fade(SKYBLUE, 0.5f));
                DrawRectangleLines(10, 10, 320, 133, BLUE);

                DrawText("Free camera default controls:", 20, 20, 10, BLACK);
                DrawText("- Mouse Wheel to Zoom in-out", 40, 40, 10, DARKGRAY);
                DrawText("- Mouse Wheel Pressed to Pan", 40, 60, 10, DARKGRAY);
                DrawText("- Alt + Mouse Wheel Pressed to Rotate", 40, 80, 10, DARKGRAY);
                DrawText("- Alt + Ctrl + Mouse Wheel Pressed for Smooth Zoom", 40, 100, 10, DARKGRAY);
                DrawText("- Z to zoom to (0, 0, 0)", 40, 120, 10, DARKGRAY);
            }

            EndDrawing();
            //----------------------------------------------------------------------------------
        }

        // De-Initialization
        //--------------------------------------------------------------------------------------
        CloseWindow(); // Close window and OpenGL context
        //--------------------------------------------------------------------------------------
    }
}