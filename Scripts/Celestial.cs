using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts;

public partial class Celestial : Node3D
{
	public double Mass;
	public double Radius;
	public double SOIRadius;
	
	public Orbit CelestialOrbit;
	public NestedPosition NestedPos = new ();
	
	public List<Celestial> ChildCelestials = [];
	public Node3D SurfaceNode;
	
	public override void _Ready()
	{
		NestedPos = CelestialOrbit != null ? 
			new NestedPosition(CelestialOrbit.GetCurrentPosition(), CelestialOrbit.ParentCelestial) 
			: new NestedPosition(Transform.Origin);

		GlobalValues.ReceiveCelestials(this);
	}
	public override void _Process(double dt)
	{
		if (CelestialOrbit != null)
		{
			NestedPos.LocalPosition = CelestialOrbit.GetCurrentPosition();
		}
		var renderSpacePos = NestedPos[CoordinateSpace.RenderSpace];
		var newScale = 1 / renderSpacePos.Magnitude();

		var extraScale = 1;
		if (SurfaceNode != null)
		{
			SurfaceNode.Visible = newScale * Radius > 1e-4;
			extraScale = SurfaceNode.Visible ? Freecam.GetDistanceIndex(this) + 1 : 1;
		}

		Scale = Vector3.One * (float)newScale * extraScale;
		Position = ((Vector3)renderSpacePos).Normalized() * extraScale;
	}

}
