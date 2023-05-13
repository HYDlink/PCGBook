// See https://aka.ms/new-console-template for more information

using System.Drawing;
using PCG.Dungeon;

void TestTextRepresent()
{
    var represent = new TextRepresent();
    represent.NewMap(10, 10);
    represent.DrawPixel(0, 3);
    represent.DrawRectangle(5, 5, 5, 5);
    represent.Show();
}

void TestBVHNode()
{
    var represent = new TextRepresent();
    var node = new BVHGenerator.BVHNode(new Rectangle(0, 0, 48, 48), represent);
    node.Gen(5);
    node.Connect();
    represent.NewMap(48, 48);
    node.Draw();
    represent.Show();
}

TestBVHNode();