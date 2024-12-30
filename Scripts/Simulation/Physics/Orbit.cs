using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Orbit
{
	public Celestial ParentCelestial;
	public Color Color;
	public ConicType OrbitType;

	public double p;
	public double e;

	public double Apoapsis;
	public double Periapsis;

	public double w;
	public double i;
	public double l;

	public double n;
	
	public double MAtEpoch;
	public double Epoch;
	
	public double Anomaly;

	public OrbitMesh OrbitLineNode;
	
	private const double Tolerance = 0.00001f;

	public Orbit()
	{
		
	}
	
	public Orbit(double _p, double _e, double _w, double _i, double _l, double _n, double _mAtEpoch, double _epoch, Celestial _ParentCelestial, Color color)
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
		CreateOrbitLine();
		Apoapsis = GetApoapsis();
		Periapsis = GetPeriapsis();
	}

	public Orbit(Orbit orbit)
	{
		p = orbit.p;
		e = orbit.p;
		w = orbit.w;
		i = orbit.i;
		l = orbit.l;
		n = orbit.n;
		MAtEpoch = orbit.MAtEpoch;
		Epoch = orbit.Epoch;
		ParentCelestial = orbit.ParentCelestial;
		Color = orbit.Color;
		OrbitType = orbit.OrbitType;
		Apoapsis = GetApoapsis();
		Periapsis = GetPeriapsis();
	}
	
	public Orbit(Vector3d position, Vector3d velocity, Celestial parentCelestial, Color color)
	{
		SetOrbitFromStateVectors(position, velocity, parentCelestial);
		Color = color;
		CreateOrbitLine();
	}

	private double GetApoapsis()
	{
		if (OrbitType is ConicType.Parabolic or ConicType.Hyperbolic)
		{
			return double.PositiveInfinity;
		}
		return p / (1 - e);
	}

	private double GetPeriapsis() { return p / (1 + e); }

	public void SetOrbitFromStateVectors(Vector3d position, Vector3d velocity, Celestial parentCelestial)
	{
		if (parentCelestial == null) return;
		ParentCelestial = parentCelestial;
		
		var angularMomentumVector = position.Cross(velocity);
		var standardGravity = GlobalValues.G * ParentCelestial.Mass;

		var eccentricityVector =
			(position * (velocity.MagnitudeSquared() - standardGravity / position.Magnitude()) -
			 position.Dot(velocity) * velocity) / standardGravity;
		
		e = eccentricityVector.Magnitude();
		
		p = angularMomentumVector.MagnitudeSquared() / standardGravity;

		var k = new Vector3d(0, 1, 0);
		var nodeVector = k.Cross(angularMomentumVector);
		
		i = Math.Acos(-angularMomentumVector.Y / angularMomentumVector.Magnitude());
		
		l = Math.Acos(nodeVector.X / nodeVector.Magnitude());
		l = nodeVector.Z < 0 ? Math.Tau - l : l;

		w = Math.Acos(nodeVector.Dot(eccentricityVector) / (nodeVector.Magnitude() * e));
		w = eccentricityVector.Y < 0 ? Math.Tau - w: w;

		var v = Math.Acos(eccentricityVector.Dot(position) / (e * position.Magnitude()));
		v = position.Dot(velocity) < 0 ? Math.Tau - v: v;

		MAtEpoch = MeanAnomalyFromTrueAnomaly(v);
		Epoch = GlobalValues.Time;
		
		OrbitType = GetConicType();
		n = OrbitType != ConicType.Parabolic? Math.Sqrt(standardGravity / Math.Pow(p / (1 - e * e) ,3)) : 2 * Math.Sqrt(standardGravity / Math.Pow(p,3));
		
		Apoapsis = GetApoapsis();
		Periapsis = GetPeriapsis();
		
		UpdateOrbitLine();
	}

	public void CreateOrbitLine()
	{
		OrbitLineNode = OrbitMesh.CreateOrbitLine(this);
	}

	public void DeleteOrbitLine()
	{
		OrbitLineNode.DeleteOrbitLine();
	}

	public void UpdateOrbitLine()
	{
		OrbitLineNode?.UpdateOrbitLine(this);
	}

	public Vector3d GetCurrentPosition()
	{
		return GetPositionAtTime(GlobalValues.Time);
	}
	
	public Vector3d GetPositionAtTime(double time, bool setAnomaly = true)
	{
		var anomaly = AnomalyFromTime(time);
		if (setAnomaly) Anomaly = anomaly;

		return PositionFromTrueAnomaly(TrueAnomalyFromMeanAnomaly(anomaly));
	}
	
	public Vector3d PositionFromTrueAnomaly(double v)
	{
		var x = p * Math.Cos(v) / (1 + e * Math.Cos(v));
		var z = p * Math.Sin(v) / (1 + e * Math.Cos(v));

		var sinW = Math.Sin(w);
		var cosW = Math.Cos(w);
		var sinL = Math.Sin(l);
		var cosL = Math.Cos(l);
		var sinI = Math.Sin(i);
		var cosI = Math.Cos(i);
		
		var xRot = x * (cosW * cosL - sinW * sinL * cosI) + z * (-sinW * cosL - cosW * sinL * cosI);
		var yRot = x * (sinW * sinI) + z * (cosW * sinI);
		var zRot = x * (cosW * sinL + sinW * cosL * cosI) + z * (-sinW * sinL + cosW * cosL * cosI);
		
		return new Vector3d(xRot, yRot, zRot);
	}
	
	public double TrueAnomalyFromMeanAnomaly(double m)
	{
		return TrueAnomalyFromAnomaly(AnomalyFromMeanAnomaly(m));
	}
	
	public double MeanAnomalyFromTrueAnomaly(double v)
	{
		return MeanAnomalyFromAnomaly(AnomalyFromTrueAnomaly(v));
	}

	public double TrueAnomalyFromTime(double time)
	{
		return AnomalyFromMeanAnomaly(MeanAnomalyFromTime(time));
	}

	public double MeanAnomalyFromTime(double time)
	{
		return n * (time - Epoch) + MAtEpoch - l - w;
	}

	public double AnomalyFromTime(double time)
	{
		return AnomalyFromMeanAnomaly(MeanAnomalyFromTime(time));
	}
	
	public double TrueAnomalyFromAnomaly(double anomaly)
	{
		switch (OrbitType)
		{
			case ConicType.Circular:
				return anomaly % Math.Tau;
			case ConicType.Elliptical:
			{
				var sinv = Math.Sqrt(1 - e * e) * Math.Sin(anomaly) / (1 - e * Math.Cos(anomaly));
				var cosv = (Math.Cos(anomaly) - e) / (1 - e * Math.Cos(anomaly));
				return Math.Atan2(sinv, cosv);
			}
			case ConicType.Hyperbolic:
			{
				var sinv = -(Math.Sqrt(e * e - 1) * Math.Sinh(anomaly)) / (1 - e * Math.Cosh(anomaly));
				var cosv = (Math.Cosh(anomaly) - e) / (1 - e * Math.Cosh(anomaly));
				return Math.Atan2(sinv, cosv);
			} 
			case ConicType.Parabolic:
				return 2 * Math.Atan(2 * anomaly);
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public double AnomalyFromMeanAnomaly(double M)
	{
		var anomaly = Anomaly;
		switch (OrbitType)
		{
			case ConicType.Circular:
				return M % Math.Tau;
			case ConicType.Elliptical:
			{
				for (int i = 0; i < 10; i++)
				{
					var dE = (M - anomaly + e * Math.Sin(anomaly)) / (1 - e * Math.Cos(anomaly));
					anomaly += dE;
					
					if (Mathf.Abs(dE) < Tolerance) break;
				}
				
				return anomaly;
			}
			case ConicType.Hyperbolic:
			{
				for (int i = 0; i < 10; i++)
				{
					var dH = (M - e * Mathf.Sinh(anomaly) + anomaly) / (e * Mathf.Cosh(anomaly) - 1);
					anomaly += dH;
					
					if (Mathf.Abs(dH) < Tolerance) break;
				}

				return anomaly;
			}
			case ConicType.Parabolic:
				return Math.Tan(Math.PI / 2 - 2 * Math.Atan(Math.Pow(Math.Tan((Math.PI / 2 - Math.Atan(1.5 * M)) / 2), 1d / 3d)));
			
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	
	public double AnomalyFromTrueAnomaly(double v)
	{
		switch (OrbitType)
		{
			case ConicType.Circular:
				return v % Math.Tau;
			case ConicType.Elliptical:
			{
				var sinE = Math.Sin(v) * Math.Sqrt(1 - e * e) / (1 + e * Math.Cos(v));
				var cosE = (e + Math.Cos(v)) / (1 + e * Math.Cos(v));
				return Math.Atan2(sinE, cosE);
			}
			case ConicType.Hyperbolic:
			{
				var sinhH = Math.Sin(v) * Math.Sqrt(e * e - 1) / (1 + e * Math.Cos(v));
				return Math.Asinh(sinhH);
			}
			case ConicType.Parabolic:
				return Math.Tan(v / 2);
			
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	
	public double MeanAnomalyFromAnomaly(double anomaly)
	{
		return OrbitType switch
		{
			ConicType.Circular => anomaly % Math.Tau,
			ConicType.Elliptical => anomaly - e * Math.Sin(anomaly),
			ConicType.Hyperbolic => e * Math.Sinh(anomaly) - anomaly,
			ConicType.Parabolic => anomaly + anomaly * anomaly * anomaly / 3,
			_ => throw new ArgumentOutOfRangeException()
		};
	}
	
	public ConicType GetConicType()
	{
		if (e < Tolerance)
		{
			return ConicType.Circular;
		}
		if (Mathf.Abs(e - 1) < Tolerance)
		{
			return ConicType.Parabolic;
		}
		if (e < 1)
		{
			return ConicType.Elliptical;
		}
		return ConicType.Hyperbolic;
	}

	public static implicit operator string(Orbit orbit)
	{
		return $"p:{orbit.p:F0}, e:{orbit.e:F3}, w:{orbit.w:F3}, i:{orbit.i:F3}, l:{orbit.l:F3}, n:{orbit.n:F20}, Parent:{orbit.ParentCelestial.Name}";
	}
}
