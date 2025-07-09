using Godot;

namespace GodotPrototype.Scripts.VesselEditor.Parts;

public partial class VesselPart : StaticBody3D
{
    
    [Export] public SnapPoint[] SnapPoints;
    [Export] public SnapPoint SurfaceAttatchPoint;

    public bool SurfaceAttatchable => SurfaceAttatchPoint != null;
}