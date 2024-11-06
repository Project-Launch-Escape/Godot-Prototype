using Godot;
using System;

public partial class CelestialScript : StaticBody3D
{
	[Export] public float Mass;
	[Export] public CelestialScript ReferenceBody;
	[Export] public float a;
	private float b;
	[Export] public float e;
	[Export] public float n;
	[Export] public float w;
	[Export] public float i;
	[Export] public float l;
	private float tol = 0.00001f;
	private float E;
	[Export] public float Radius;
	[Export] public int CoordLayer;
	public Vector3 LocalPos;

	public override void _Ready()
	{
		b = a * (1 - Mathf.Pow(e, 2));
		if (ReferenceBody != null)
		{
			n = Mathf.Sqrt(GlobalValues.G * ReferenceBody.Mass /( a * GlobalValues.GetRefConversionFactor(CoordLayer, 0))) / (a * GlobalValues.GetRefConversionFactor(CoordLayer, 0)); // Don't forget all angles are in radians
		}
		else
		{
			LocalPos = Transform.Origin;
		}
		Transform = Transform.Scaled(new Vector3(Radius, Radius, Radius));
	}
	public Vector3 GetPosition(int Layer)
	{
		// Layer 1 is GalaxySpace, 2 is StarSpace, 3 is PlanetSpace, and 4+ are levels of nested PlanetSpace. To get the position in Layer 0 (RenderSpace), Use the "GetRenderSpacePos()" Method
		if (Layer == CoordLayer)
		{
			return LocalPos;
		}
		else if (Layer < CoordLayer)
		{
			return ReferenceBody.GetPosition(Layer);
		}
		else
		{
			return new Vector3();
		}
	}
	public Vector3 GetRenderSpacePos()
	{
		Vector3 PosAbsolute = new Vector3();
		PosAbsolute += GetPosition(3) * GlobalValues.GetRefConversionFactor(3, 0) * GlobalValues.Scale;
		PosAbsolute += GetPosition(2) * GlobalValues.GetRefConversionFactor(2, 0) * GlobalValues.Scale;
		PosAbsolute += GetPosition(1) * GlobalValues.GetRefConversionFactor(1, 0) * GlobalValues.Scale;
		return PosAbsolute;
	}
	public override void _Process(double dt)
	{
		if (ReferenceBody != null)
		{
			float E = CalculateEccentricAnomaly(-n * GlobalValues.time + w);
			float x = a * (Mathf.Cos(E)) - a + b;
			float z = b * Mathf.Sin(E);

			float x_Rot = x * (Mathf.Cos(w) * Mathf.Cos(l) - Mathf.Sin(w) * Mathf.Sin(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Cos(l) - Mathf.Cos(w) * Mathf.Sin(l) * Mathf.Cos(i));
			float y_Rot = x * (Mathf.Sin(w) * Mathf.Sin(i)) + z * (Mathf.Cos(w) * Mathf.Sin(i));
			float z_Rot = x * (Mathf.Cos(w) * Mathf.Sin(l) + Mathf.Sin(w) * Mathf.Cos(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Sin(l) + Mathf.Cos(w) * Mathf.Cos(l) * Mathf.Cos(i));

			LocalPos = new Vector3(x_Rot, y_Rot, z_Rot);
		}
		Transform = new Transform3D(Transform.Basis, GetRenderSpacePos());
	}
	float CalculateEccentricAnomaly(float M)
	{
		for (int i = 0; i < 10; i++)
		{
			float dE = (E - e * Mathf.Sin(E) - M) / (1 - e * Mathf.Cos(E));
			E -= dE;
			if (Mathf.Abs(dE) < tol)
			{
				break;
			}
		}
		return E;
	}
}
