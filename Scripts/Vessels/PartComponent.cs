using Godot;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass]
public abstract partial class PartComponent : Node3D
{
	public VesselPart ParentPart;
	public Vessel ParentVessel => ParentPart.ParentVessel;
	public PartTree ParentTree => ParentPart.ParentTree;
	public FuelSystem ConnectedFuelSystem => ParentPart.ConnectedFuelSystem;

	[Export] public double BaseMass { get; private set; }
	public double Mass => GetMass();

	protected virtual double GetMass() => BaseMass;
	public ComponentType ComponentType => GetComponentType();

	private ComponentType GetComponentType()
	{
		return this switch
		{
			FuelTank => ComponentType.FuelTank,
			RocketEngine => ComponentType.Engine,
			Converter => ComponentType.Converter,
			Decoupler => ComponentType.Decoupler,
			_ => throw new IndexOutOfRangeException()
		};
	}
}

public enum ComponentType
{
	FuelTank,
	Engine,
	Converter,
	Default,
	Decoupler
}
