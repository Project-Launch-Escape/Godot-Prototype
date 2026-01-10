using Godot;

namespace GodotPrototype.Scripts.Vessels;

public partial class FuelPipe : MeshInstance3D
{
	public VesselPart ParentPart;
	public VesselPart ChildPart;

	public FuelSystem ConnectedSystem;
	
	
}
