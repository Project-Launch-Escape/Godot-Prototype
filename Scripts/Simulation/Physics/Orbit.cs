using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Simulation.DoublePrecision;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Orbit
{
	public CelestialScript ParentCelestial;
	public Color Color;
	public ConicType OrbitType;
	
	public double p;
	public double e;

	public double w;
	public double i;
	public double l;

	public double n;
	public double Anomaly;

	public double MAtEpoch;
	public double Epoch;
	
	private const double Tol = 0.00001f;

	public Orbit(double _p, double _e, double _w, double _i, double _l, double _n, double _mAtEpoch, double _epoch, CelestialScript _ParentCelestial, Color color)
	{
		p = _p;
		e = _e;
		w = _w;
		i = _i;
		l = _l;
		n = _n;
		MAtEpoch = _mAtEpoch;
		Epoch = _epoch;
		ParentCelestial = _ParentCelestial;
		Color = color;
		OrbitType = GetConicType();
	}

	public Orbit(Vector3d position, Vector3d velocity, CelestialScript _ParentCelestial, Color color)
	{
		var coordLayer = _ParentCelestial.NestedPos.CoordLayer.Increment();
		var angularMomentumVector = position.Cross(velocity);
		var standardGravity = GlobalValues.G * _ParentCelestial.Mass;
		
		var eccentricityVector = velocity.Cross(angularMomentumVector) / standardGravity - position.Normalized();
		e = eccentricityVector.Magnitude();
		OrbitType = GetConicType();
		
		var orbitEnergy = Math.Abs(velocity.MagnitudeSquared() / 2 - standardGravity / position.Magnitude());
		
		p = angularMomentumVector.MagnitudeSquared() / standardGravity;

		var K = new Vector3d(0, 1, 0);
		var nodeVector = K.Cross(angularMomentumVector);
		
		i = Math.Acos(K.Dot(angularMomentumVector) / angularMomentumVector.Magnitude());
		
		l = Math.Acos(nodeVector.X / nodeVector.Magnitude());
		l = nodeVector.Y < 0 ? Math.Tau - l : l;
		
		w = Math.Atan2(eccentricityVector.Z, eccentricityVector.X) - l;
		w = w < 0 ? w + Math.Tau : w;
		
		n = OrbitType != ConicType.Parabolic? Math.Sqrt(standardGravity / Math.Pow(p / (1 - e * e) ,3)) : 2 * Math.Sqrt(standardGravity / Math.Pow(p,3));
		
		Color = color;
		ParentCelestial = _ParentCelestial;
	}
	
	public Vector3d GetPosition()
	{
		return GetPositionAtV(CalculateTrueAnomaly(n * (GlobalValues.Time - Epoch) + MAtEpoch - l - w));
	}
	public Vector3d GetPositionAtV(double v)
	{
		var x = p * Math.Cos(v) / (1 + e * Math.Cos(v));
		var z = p * Math.Sin(v) / (1 + e * Math.Cos(v));
		
		var xRot = x * (Math.Cos(w) * Math.Cos(l) - Math.Sin(w) * Math.Sin(l) * Math.Cos(i)) + z * (-Math.Sin(w) * Math.Cos(l) - Math.Cos(w) * Math.Sin(l) * Math.Cos(i));
		var yRot = x * (Math.Sin(w) * Math.Sin(i)) + z * (Math.Cos(w) * Math.Sin(i));
		var zRot = x * (Math.Cos(w) * Math.Sin(l) + Math.Sin(w) * Math.Cos(l) * Math.Cos(i)) + z * (-Math.Sin(w) * Math.Sin(l) + Math.Cos(w) * Math.Cos(l) * Math.Cos(i));
		
		return new Vector3d(xRot, yRot, zRot);
	}
	
	private double CalculateTrueAnomaly(double M)
	{
		if (OrbitType == ConicType.Circular)
		{
			return M % Math.Tau;
		}
		if (OrbitType == ConicType.Elliptical)
		{
			for (int i = 0; i < 10; i++)
			{
				var dE = (M - Anomaly + e * Math.Sin(Anomaly)) / (1 - e * Math.Cos(Anomaly));
				Anomaly += dE;
				if (Mathf.Abs(dE) < Tol)
				{
					break;
				}
			}
			var sinv = Math.Sqrt(1 - e * e) * Math.Sin(Anomaly) / (1 - e * Math.Cos(Anomaly));
			var cosv = (Math.Cos(Anomaly) - e) / (1 - e * Math.Cos(Anomaly));
			return Math.Atan2(sinv, cosv);
		}
		if (OrbitType == ConicType.Hyperbolic)
		{
			for (int i = 0; i < 10; i++)
			{
				var dH = (M - e * Mathf.Sinh(Anomaly) + Anomaly) / (e * Mathf.Cosh(Anomaly) - 1);
				Anomaly += dH;
				if (Mathf.Abs(dH) < Tol)
				{
					break;
				}
			}
			var sinv = -(Math.Sqrt(e * e - 1) * Math.Sinh(Anomaly)) / (1 - e * Math.Cosh(Anomaly));
			var cosv = (Math.Cosh(Anomaly) - e) / (1 - e * Math.Cosh(Anomaly));
			GD.Print(Math.Atan2(sinv, cosv));
			return Math.Atan2(sinv, cosv);
		}
		
		return 2 * Math.Atan(2 * Math.Tan(Math.PI / 2 - 2 * Math.Atan(Math.Pow(Math.Tan((Math.PI / 2 - Math.Atan(1.5 * M)) / 2), 1d / 3d))));
	}
	
	private ConicType GetConicType()
	{
		if (e < Tol)
		{
			return ConicType.Circular;
		}
		if (Mathf.Abs(e - 1) < Tol)
		{
			return ConicType.Parabolic;
		}
		if (e < 1)
		{
			return ConicType.Elliptical;
		}
		return ConicType.Hyperbolic;
	}
}
