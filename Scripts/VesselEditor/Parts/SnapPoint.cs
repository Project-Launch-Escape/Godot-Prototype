using Godot;

namespace GodotPrototype.Scripts.VesselEditor.Parts;

[GlobalClass]
public partial class SnapPoint : Resource
{
    [Export]
    public Vector3 Position;
    
    [Export]
    public Vector3 Rotation;
    
    public Quaternion Orientation => Quaternion.FromEuler(new Vector3(Mathf.DegToRad(Rotation.X), Mathf.DegToRad(Rotation.Y), Mathf.DegToRad(Rotation.Z)));
}