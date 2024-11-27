using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Simulation.DoublePrecision;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Orbit
{
	public CelestialScript ParentCelestial;
	public Color Color;
	
	public double a;
	public double b;
	public double e;

	public double w;
	public double i;
	public double l;

	public double n;
	public double E;

	public double MAtEpoch;
	public double Epoch;
	
	private static double tol = 0.00001f;

	public Orbit(double _a, double _e, double _w, double _i, double _l, double _n, double _mepoch, double _epoch, CelestialScript _ParentCelestial, Color color)
	{
		a = _a;
		b = _a * Mathf.Sqrt(1 - _e * _e);
		e = _e;
		w = _w;
		i = _i;
		l = _l;
		n = _n;
		MAtEpoch = _mepoch;
		Epoch = _epoch;
		ParentCelestial = _ParentCelestial;
		Color = color;
	}

	public Orbit(Vector3d position, Vector3d velocity, CelestialScript _ParentCelestial, Color color)
	{
		var coordLayer = _ParentCelestial.CoordLayer.Increment();
		var angularMomentumVector = position.Cross(velocity);
		var standardGravity = GlobalValues.G * _ParentCelestial.Mass;
		
		var eccentricityVector = velocity.Cross(angularMomentumVector) / standardGravity - position.Normalized();
		e = eccentricityVector.Magnitude();

		var orbitEnergy = Mathf.Abs(velocity.MagnitudeSquared() / 2 - standardGravity / position.Magnitude());
		if (e < 1)
		{
			a = standardGravity / (2 * orbitEnergy);
			b = a * Mathf.Sqrt(1 - e * e);
		}
		else
		{
			a = double.PositiveInfinity;
			b = angularMomentumVector.MagnitudeSquared() / standardGravity;
		}
		
		var nodeVector = new Vector3d(0, -1, 0).Cross(angularMomentumVector);
		i = Mathf.Acos(angularMomentumVector.Y / angularMomentumVector.Magnitude());
		l = Mathf.Acos(nodeVector.X / nodeVector.Magnitude());
		l = l < 0 ? l + Math.Tau : l;
		w = Mathf.Atan2(eccentricityVector.Z, eccentricityVector.X) - l;
		w = w < 0 ? w + Math.Tau : w;
		
		n = Mathf.Sqrt(GlobalValues.G * _ParentCelestial.Mass / a) / a;
		ParentCelestial = _ParentCelestial;
		a *= CoordinateSpace.RenderSpace.GetConversionFactor(coordLayer);
		b *= CoordinateSpace.RenderSpace.GetConversionFactor(coordLayer);
		Color = color;
	}
	
	public Vector3d GetPosition()
	{
		return GetPositionAtE(CalculateEccentricAnomaly((n * (GlobalValues.Time - Epoch) + MAtEpoch - l - w)  % Math.Tau));
	}
	public Vector3d GetPositionAtE(double E)
	{
		var x = a * (Mathf.Cos(E) - e);
		var z = b * Mathf.Sin(E);

		var xRot = x * (Mathf.Cos(w) * Mathf.Cos(l) - Mathf.Sin(w) * Mathf.Sin(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Cos(l) - Mathf.Cos(w) * Mathf.Sin(l) * Mathf.Cos(i));
		var yRot = x * (Mathf.Sin(w) * Mathf.Sin(i)) + z * (Mathf.Cos(w) * Mathf.Sin(i));
		var zRot = x * (Mathf.Cos(w) * Mathf.Sin(l) + Mathf.Sin(w) * Mathf.Cos(l) * Mathf.Cos(i)) + z * (-Mathf.Sin(w) * Mathf.Sin(l) + Mathf.Cos(w) * Mathf.Cos(l) * Mathf.Cos(i));
		
		return new Vector3d(xRot, yRot, zRot);
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
