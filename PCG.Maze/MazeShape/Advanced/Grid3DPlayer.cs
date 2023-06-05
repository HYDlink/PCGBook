using System.Numerics;
using static Raylib_cs.Raylib;
using Color = Raylib_cs.Color;

namespace PCG.Maze.MazeShape.Advanced;

public class Grid3DPlayer : RaylibPlayer
{
    private Grid3D Grid3D;
    public int Width => Grid3D.Width;
    public int Height => Grid3D.Height;
    public int Length => Grid3D.Length;

    public Grid3DCell?[,,] Cells => Grid3D.Cells;
    private Vector3 cubeSize = new Vector3(1, 1, 1);

    public Grid3DPlayer(Grid3D grid3D)
    {
        Grid3D = grid3D;
    }


    public override void Draw()
    {
        Vector3 GetCellCenterPos(int x, int y, int z)
        {
            var pos = (new Vector3(x - Width / 2, y - Height / 2, z - Height / 2) * 2 + Vector3.One) * cubeSize;
            return pos;
        }

        void DrawCorridor(Vector3 start, Vector3 end)
            => DrawCapsule(start, end, 0.2f, 1, 2, Color.SKYBLUE);


        void DrawMaze()
        {
            for (var z = 0; z < Length; z++)
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var cell = Cells[z, y, x];
                if (cell is null) continue;
                var pos = GetCellCenterPos(x, y, z);

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

                DrawCube(pos, 1.0f, 1.0f, 1.0f, Color.RED);
                DrawCubeWires(pos, 1.0f, 1.0f, 1.0f, Color.MAROON);
            }
        }

        DrawMaze();

        DrawGrid(10, 1.0f);
    }

    public override void Init()
    {
        CameraSize = 4f * Width;
    }
}