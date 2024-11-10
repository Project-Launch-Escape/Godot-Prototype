using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts;

public partial class CelestialScript : StaticBody3D
{
	[Export] public float Mass;
	[Export] public CelestialScript ParentCelestial;
	[Export] public float a;
	private float b;
	[Export] public float e;
	[Export] public float n;
	[Export] public float w;
	[Export] public float i;
	[Export] public float l;
	private float tol = 0.00001f;
	private double E;
	[Export] public float Radius;
	[Export] public float SOIRadius;
	[Export] public CoordinateSpace CoordLayer;
	public NestedPosition NestedPos;

	public override void _Ready()
	{
		b = a * (1 - Mathf.Pow(e, 2));
		if (ParentCelestial != null)
		{
			n = Mathf.Sqrt(GlobalValues.G * ParentCelestial.Mass /( a * GlobalValues.GetRefConversionFactor(CoordLayer, 0))) / (a * GlobalValues.GetRefConversionFactor(CoordLayer, 0)); // Don't forget all angles are in radians
			SOIRadius = a * Mathf.Pow(Mass / ParentCelestial.Mass, 0.4f);
		}
		else
		{
			NestedPos = new NestedPosition(Transform.Origin);
		}
		Transform = Transform.Scaled(new Vector3(Radius, Radius, Radius));
		GlobalValues.ReceiveCelestials(this);
	}

	public override void _Process(double dt)
	{
		if (ParentCelestial != null)
		{
			float E = (float)CalculateEccentricAnomaly((-n * GlobalValues.Time + w) % Math.Tau);
			float x = a * (Mathf.Cos(E)) - a + b;
			float z = b * Mathf.Sin(E);

			float x_Rot = x * (Mathf.Cos(w) * Mathf.Cos(l) - Mathf.Sin(w) * Mathf.Sin(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Cos(l) - Mathf.Cos(w) * Mathf.Sin(l) * Mathf.Cos(i));
			float y_Rot = x * (Mathf.Sin(w) * Mathf.Sin(i)) + z * (Mathf.Cos(w) * Mathf.Sin(i));
			float z_Rot = x * (Mathf.Cos(w) * Mathf.Sin(l) + Mathf.Sin(w) * Mathf.Cos(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Sin(l) + Mathf.Cos(w) * Mathf.Cos(l) * Mathf.Cos(i));

			NestedPos = new NestedPosition(new Vector3(x_Rot, y_Rot, z_Rot), ParentCelestial);
		}
		Transform = new Transform3D(Transform.Basis, NestedPos.GetPositionAtLayer(CoordinateSpace.RenderSpace));
	}
	double CalculateEccentricAnomaly(double M)
	{
		for (int i = 0; i < 10; i++)
		{
			double dE = (E - e * Mathf.Sin(E) - M) / (1 - e * Mathf.Cos(E));
			E -= dE;
			if (Mathf.Abs(dE) < tol)
			{
				break;
			}
		}
		return E;
	}
}