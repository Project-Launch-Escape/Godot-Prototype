using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts;

public partial class CelestialScript : Node3D
{
	[Export] public double Mass;
	[Export] public double Radius;
	[Export] public double SOIRadius;
	public Orbit CelestialOrbit;
	public NestedPosition NestedPos = new ();
	
	public override void _Ready()
	{
		if (CelestialOrbit != null)
		{
			OrbitLineMeshGenerator.CreateOrbitLine(CelestialOrbit);
			NestedPos = new NestedPosition(CelestialOrbit.GetPosition(), CelestialOrbit.ParentCelestial);
		}
		else
		{
			NestedPos = new NestedPosition(Transform.Origin);
		}
		GlobalValues.ReceiveCelestials(this);
	}
	public override void _Process(double dt)
	{
		if (CelestialOrbit != null)
		{
			NestedPos.LocalPosition = CelestialOrbit.GetPosition();
		}
		var renderSpacePos = NestedPos[CoordinateSpace.RenderSpace];
		var newRadius = Radius * 2 / renderSpacePos.Magnitude();
		
		Visible = newRadius > 1e-4;

		var extraScale = Visible ? Freecam.GetDistanceIndex(this) + 1 : 1;

		Scale = Vector3.One * (float)newRadius * extraScale;
		Position = ((Vector3)renderSpacePos).Normalized() * extraScale;
	}

}
