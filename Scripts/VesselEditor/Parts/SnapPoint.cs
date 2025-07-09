using Godot;

namespace GodotPrototype.Scripts.VesselEditor.Parts;

[GlobalClass]
public partial class SnapPoint : Node3D
{
    public bool Occupied;
    [Export] public Area3D Collider;
    public VesselPart Parent => (VesselPart)GetParentNode3D();

    public VesselPart AttachedPart;
    public SnapPoint AttachedSnapPoint;
    
    public Quaternion Orientation => Quaternion.FromEuler(GlobalRotation).Normalized();
    public Quaternion LocalOrientation => Quaternion.FromEuler(Rotation).Normalized();

    public void AttachTo(SnapPoint otherSnapPoint)
    {
        AttachedPart = otherSnapPoint.Parent;
        AttachedSnapPoint = otherSnapPoint;
        Occupied = true;
    }

    public void Unattatch()
    {
        AttachedSnapPoint = null;
        AttachedPart = null;
        Occupied = false;
    }
    
}