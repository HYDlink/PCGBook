using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCG.Dungeon;

namespace PCG.GUI;

public partial class DungeonGeneratorViewModel : ObservableObject
{
    [ObservableProperty] private int width;
    [ObservableProperty] private int height;
    [ObservableProperty] private int depth = 4;

    private Represent represent;

    public DungeonGeneratorViewModel(DungeonGeneratorWindow window)
    {
        represent = new GeometryGroupRepresent(window.Canvas);
    }

    [RelayCommand]
    private void Draw()
    {
        var generator = new BVHGenerator(width, height, represent);
        generator.Gen(depth);
    }
}