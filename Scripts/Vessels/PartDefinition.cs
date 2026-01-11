using Godot;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass]
public partial class PartDefinition : Resource
{
    [Export] public PackedScene PartScene;
    [Export] public string PartName;
    [Export] public int PartID;

    [Export] public Texture2D Icon;
    [Export] public Color IconColor;
}