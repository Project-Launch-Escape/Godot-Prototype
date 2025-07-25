using Godot;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass]
public abstract partial class PartComponent : Node3D
{
	public VesselPart ParentPart;
	public Vessel ParentVessel => ParentPart.ParentVessel;
	public FuelSystem ConnectedFuelSystem => ParentPart.ConnectedFuelSystem;

	[Export] protected double BaseMass;
	public double Mass => GetMass();

	protected virtual double GetMass() => BaseMass;
}
