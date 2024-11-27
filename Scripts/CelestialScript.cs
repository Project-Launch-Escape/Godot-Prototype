using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts;

public partial class CelestialScript : StaticBody3D
{
	[Export] public double Mass;
	[Export] public CelestialScript ParentCelestial;
	[Export] private double a;
	[Export] private double e;
	[Export] private double n;
	[Export] private double w;
	[Export] private double i;
	[Export] private double l;
	[Export] private double mAtEpoch;
	[Export] private double epoch;
	private double E;
	
	[Export] public double Radius;
	[Export] public double SOIRadius;
	[Export] public CoordinateSpace CoordLayer;
	public Orbit CelestialOrbit;
	public NestedPosition NestedPos;
	[Export] public Color CelestialColor;

	public override void _Ready()
	{
		if (ParentCelestial != null)
		{
			if (n == 0) n = Mathf.Sqrt(GlobalValues.G * ParentCelestial.Mass / ( a * CoordLayer.GetConversionFactor(0)) ) / (a * CoordLayer.GetConversionFactor(0));
			CelestialOrbit = new Orbit(a, e, w, i, l, n, mAtEpoch, epoch, ParentCelestial, CelestialColor);
			OrbitLineMeshGenerator.CreateOrbitLine(CelestialOrbit);
			
			NestedPos = new NestedPosition(GetPosition(),ParentCelestial);
			SOIRadius = a * Mathf.Pow(Mass / ParentCelestial.Mass, 0.4f);
		}
		else
		{
			NestedPos = new NestedPosition(Transform.Origin);
		}
		Transform = Transform.Scaled(Vector3.One * (float)Radius * (float)CoordLayer.Increment().GetConversionFactor(CoordinateSpace.RenderSpace));
		GlobalValues.ReceiveCelestials(this);
	}
	public override void _Process(double dt)
	{
		if (ParentCelestial != null)
		{
			NestedPos.LocalPosition = CelestialOrbit.GetPosition();
		}
		
		var renderSpacePos = NestedPos[CoordinateSpace.RenderSpace];
		var newRadius = Radius * CoordLayer.Increment().GetConversionFactor(CoordinateSpace.RenderSpace) / renderSpacePos.Magnitude();
		
		Visible = newRadius > 1e-4;

		var extraScale = Visible ? Freecam.GetDistanceIndex(this) + 1 : 1;

		Position = ((Vector3)renderSpacePos).Normalized() * extraScale;
		Scale = Vector3.One * (float)newRadius * extraScale;
	}

}
