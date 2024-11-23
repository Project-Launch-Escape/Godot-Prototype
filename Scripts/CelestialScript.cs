using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Simulation.DoublePrecision;

namespace GodotPrototype.Scripts;

public partial class CelestialScript : StaticBody3D
{
	[Export] public double Mass;
	[Export] public CelestialScript ParentCelestial;
	[Export] private double a;
	private double b;
	[Export] private double e;
	[Export] private double n;
	[Export] private double w;
	[Export] private double i;
	[Export] private double l;
	private static double tol = 0.00001f;
	private double E;
	[Export] public double Radius;
	[Export] public double SOIRadius;
	[Export] public CoordinateSpace CoordLayer;
	public NestedPosition NestedPos;
	[Export] public Color CelestialColor;

	public override void _Ready()
	{
		b = a * (1 - Mathf.Pow(e, 2));
		if (ParentCelestial != null)
		{
			if (n == 0) n = Mathf.Sqrt(GlobalValues.G * ParentCelestial.Mass / ( a * CoordLayer.GetConversionFactor(0)))
							/ (a * CoordLayer.GetConversionFactor(0));

			NestedPos = new NestedPosition(GetPositionAtE(E),ParentCelestial);
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
			CalculateEccentricAnomaly((n * GlobalValues.Time) % Math.Tau);
			NestedPos.LocalPosition = GetPositionAtE(E);
		}
		
		var renderSpacePos = NestedPos[CoordinateSpace.RenderSpace];
		var newRadius = Radius * CoordLayer.Increment().GetConversionFactor(CoordinateSpace.RenderSpace) / renderSpacePos.Magnitude();
		
		Visible = newRadius > 1e-4;

		var extraScale = Visible ? Freecam.GetDistanceIndex(this) + 1 : 1;

		Position = (Vector3)renderSpacePos.Normalized() * extraScale;
		Scale = Vector3.One * (float)newRadius * extraScale;
	}

	public Vector3d GetPositionAtE(double E)
	{
		var x = a * Mathf.Cos(E) - a + b;
		var z = b * Mathf.Sin(E);

		var x_Rot = x * (Mathf.Cos(w) * Mathf.Cos(l) - Mathf.Sin(w) * Mathf.Sin(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Cos(l) - Mathf.Cos(w) * Mathf.Sin(l) * Mathf.Cos(i));
		var y_Rot = x * (Mathf.Sin(w) * Mathf.Sin(i)) + z * (Mathf.Cos(w) * Mathf.Sin(i));
		var z_Rot = x * (Mathf.Cos(w) * Mathf.Sin(l) + Mathf.Sin(w) * Mathf.Cos(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Sin(l) + Mathf.Cos(w) * Mathf.Cos(l) * Mathf.Cos(i));
		return new Vector3d((float)x_Rot, (float)y_Rot, (float)z_Rot);
	}
	private double CalculateEccentricAnomaly(double M)
	{
		for (int i = 0; i < 10; i++)
		{
			var dE = (E - e * Mathf.Sin(E) - M) / (1 - e * Mathf.Cos(E));
			E -= dE;
			if (Mathf.Abs(dE) < tol)
			{
				break;
			}
		}
		return E;
	}
}
