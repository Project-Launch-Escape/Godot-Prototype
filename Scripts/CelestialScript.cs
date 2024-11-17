using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
namespace GodotPrototype.Scripts;

public partial class CelestialScript : StaticBody3D
{
	[Export] public float Mass;
	[Export] public CelestialScript ParentCelestial;
	[Export] private float a;
	private float b;
	[Export] private float e;
	[Export] private float n;
	[Export] private float w;
	[Export] private float i;
	[Export] private float l;
	private static float tol = 0.00001f;
	private double E;
	[Export] public float Radius;
	[Export] public float SOIRadius;
	[Export] public CoordinateSpace CoordLayer;
	public NestedPosition NestedPos;
	[Export] public Color CelestialColor;

	public override void _Ready()
	{
		b = a * (1 - Mathf.Pow(e, 2));
		if (ParentCelestial != null)
		{
			if (n == 0)
			{
				n = Mathf.Sqrt(GlobalValues.G * ParentCelestial.Mass /( a * GlobalValues.GetRefConversionFactor(CoordLayer, 0))) / (a * GlobalValues.GetRefConversionFactor(CoordLayer, 0));
			}

			NestedPos = new NestedPosition(GetPositionAtE(E),ParentCelestial);
			SOIRadius = a * Mathf.Pow(Mass / ParentCelestial.Mass, 0.4f);
		}
		else
		{
			NestedPos = new NestedPosition(Transform.Origin);
		}
		Transform = Transform.Scaled(new Vector3(1,1,1) * Radius * CoordLayer.Increment().GetConversionFactor(CoordinateSpace.RenderSpace) * GlobalValues.Scale);
		GlobalValues.ReceiveCelestials(this);
	}
	public override void _Process(double dt)
	{
		if (ParentCelestial != null)
		{
			CalculateEccentricAnomaly((n * GlobalValues.Time) % Math.Tau);
			NestedPos.LocalPosition = GetPositionAtE(E);
		}

		var desiredCameraDist = 1;
		var renderSpacePos = NestedPos.GetPositionAtLayer(CoordinateSpace.RenderSpace);
		var newRadius = Radius * desiredCameraDist * CoordLayer.Increment().GetConversionFactor(CoordinateSpace.RenderSpace) * GlobalValues.Scale / renderSpacePos.Length();
		var newBasis = Basis.Identity.Scaled(new Vector3(1,1,1) * newRadius);
		
		Transform = new Transform3D(newBasis, renderSpacePos.Normalized() * desiredCameraDist);
	}

	public Vector3 GetPositionAtE(double E)
	{
		var x = a * Mathf.Cos(E) - a + b;
		var z = b * Mathf.Sin(E);

		var x_Rot = x * (Mathf.Cos(w) * Mathf.Cos(l) - Mathf.Sin(w) * Mathf.Sin(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Cos(l) - Mathf.Cos(w) * Mathf.Sin(l) * Mathf.Cos(i));
		var y_Rot = x * (Mathf.Sin(w) * Mathf.Sin(i)) + z * (Mathf.Cos(w) * Mathf.Sin(i));
		var z_Rot = x * (Mathf.Cos(w) * Mathf.Sin(l) + Mathf.Sin(w) * Mathf.Cos(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Sin(l) + Mathf.Cos(w) * Mathf.Cos(l) * Mathf.Cos(i));
		return new Vector3((float)x_Rot, (float)y_Rot, (float)z_Rot);
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
