using Godot;

namespace GodotPrototype.Scripts.VesselEditor.Parts;

[GlobalClass]
public partial class PartDefinition : Resource
{
    [Export]
    public PackedScene Asset;
}