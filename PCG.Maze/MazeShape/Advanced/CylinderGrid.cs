using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using Color = Raylib_cs.Color;

namespace PCG.Maze.MazeShape.Advanced;

public class CylinderGrid : Grid
{
    public CylinderGrid(int width, int height) : base(width, height)
    {
    }

    public CylinderGrid(MaskGrid maskGrid) : base(maskGrid)
    {
    }

    protected override void SetAllCellsNeighbor()
    {
        base.SetAllCellsNeighbor();
        var max_right = Width - 1;
        for (var y = 0; y < Height; y++)
        {
            var left = Cells[y, max_right];
            var right = Cells[y, 0];
            left.Right = right;
            right.Left = left;
        }
    }
}

public class CylinderPlayer : RaylibPlayer
{
    private Mesh cylinder;
    private Texture2D texture;
    private CylinderGrid CylinderGrid;

    public float CellWidth = 0.5f;

    public CylinderPlayer(CylinderGrid cylinder)
    {
        CylinderGrid = cylinder;
        height = cylinder.Height * CellWidth;
        radius = cylinder.Width * CellWidth / (2 * Single.Pi);
        CameraSize = 1.5f * MathF.Max(radius, height);
    }

    protected override int screenHeight => 1080;
    protected override int screenWidth => 480;

    private string filePath = @"C:\Work\Projects\PCGBook\PCG.Maze\bin\Debug\net7.0\CylinderGridDistance.png";

    public CylinderPlayer LoadImage(string filePath)
    {
        this.filePath = filePath;
        return this;
    }

    private float height = 10;
    private float radius;
    private Model model;

    public override void Init()
    {
        var texture = LoadTexture(filePath);
        var cylinder = GenMeshCylinder(radius, height, 32);

        model = LoadModelFromMesh(cylinder);
        unsafe
        {
            var material = model.materials;
            material[0].maps[(int)MaterialMapIndex.MATERIAL_MAP_ALBEDO].texture = texture;
        }
    }

    public override void Draw()
    {
        var pos = new Vector3(0, - height / 2, 0);
        DrawModel(model, pos, 1, Color.WHITE);
        // DrawGrid(10, 1.0f);
    }
}