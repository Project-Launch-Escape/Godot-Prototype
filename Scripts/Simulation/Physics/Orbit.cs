using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Orbit : OrbitalElements
{
	public Celestial Primary;
	public Color Color;

	private OrbitMesh _orbitLineNode;

	public bool IsEscapeTrajectory => FindIfIsEscapeTrajectory();
	public Range TrueAnomalyRange => GetTrueAnomalyRange();


	public Orbit()
	{
		
	}
	
	public Orbit(double p, double e, double w, double i, double l, double n, double t, Celestial primary, Color color)
	{
		this.p = p;
		this.e = e;
		this.w = w;
		this.i = i;
		this.l = l;
		this.n = n;
		T = t;
		Primary = primary;
		Color = color;
		OrbitType = GetConicType();
	}

	public Orbit(Orbit orbit)
	{
		SetFromOrbit(orbit);
	}
	
	public Orbit(Vector3d position, Vector3d velocity, Celestial primary, Color color, double? epoch = null)
	{
		SetFromStateVectors(position, velocity, primary, epoch);
		Color = color;
	}

	public void SetFromOrbit(Orbit orbit)
	{
		p = orbit.p;
		e = orbit.e;
		w = orbit.w;
		i = orbit.i;
		l = orbit.l;
		n = orbit.n;
		T = orbit.T;
		Primary = orbit.Primary;
		Color = orbit.Color;
		OrbitType = orbit.OrbitType;
	}
	
	public void SetFromStateVectors(Vector3d position, Vector3d velocity, Celestial primary, double? epoch = null)
	{
		if (primary == null) return;
		Primary = primary;
		
		epoch ??= GlobalValues.Time;
		
		var angularMomentumVector = position.Cross(velocity);
		var mu = Primary.Mu;

		var eccentricityVector =
			(position * (velocity.MagnitudeSquared() - mu / position.Magnitude) -
			 position.Dot(velocity) * velocity) / mu;
		
		e = eccentricityVector.Magnitude;
		
		p = angularMomentumVector.MagnitudeSquared() / mu;
		
		var nodeVector = Vector3d.K.Cross(angularMomentumVector);
		
		i = Math.Acos(-angularMomentumVector.Y / angularMomentumVector.Magnitude);
		if (double.IsNaN(i)) i = 0;
		
		l = Math.Acos(nodeVector.X / nodeVector.Magnitude);
		l = nodeVector.Z < 0 ? Math.Tau - l : l;
		if (double.IsNaN(l)) l = 0;

		w = Math.Acos(nodeVector.Dot(eccentricityVector) / (nodeVector.Magnitude * e));
		w = eccentricityVector.Y < 0 ? Math.Tau - w: w;
		if (double.IsNaN(w)) w = 0;

		var v = Math.Acos(eccentricityVector.Dot(position) / (e * position.Magnitude));
		v = position.Dot(velocity) < 0 ? Math.Tau - v: v;
		
		OrbitType = GetConicType();
		
		n = OrbitType is not ConicType.Parabolic? Math.Sqrt(mu / Math.Pow(p / Math.Abs(1 - e * e) ,3)) : (2 * Math.Sqrt(mu / Math.Pow(p,3)));
		T = epoch.Value - MeanAnomalyFromTrueAnomaly(v) / n;
		
		UpdateOrbitLine();
	}

	private bool FindIfIsEscapeTrajectory() => Apoapsis > Primary?.SOIRadius;
	
	private Range GetTrueAnomalyRange()
	{
		var maxV = IsEscapeTrajectory ? TrueAnomalyFromDistance(Primary.SOIRadius) : Math.PI;
		
		return new Range(-maxV, maxV);
	}
	
	
	public void CreateOrbitLine(Range? trueAnomalyRange = null)
	{
		if (_orbitLineNode != null) return;
		_orbitLineNode = OrbitMesh.CreateOrbitLine(this, trueAnomalyRange);
	}
	public void DeleteOrbitLine()
	{
		_orbitLineNode?.DeleteOrbitLine();
	}
	public void UpdateOrbitLine(Range? trueAnomalyRange = null)
	{
		_orbitLineNode?.UpdateOrbitLine(this, trueAnomalyRange);	
	}

	public void CreateApoapsisMarker()
	{
		if (IsEscapeTrajectory) return;
		_orbitLineNode?.CreateMarker("Apoapsis", Math.PI, "Ap");
	}
	public void CreatePeriapsisMarker()
	{
		_orbitLineNode?.CreateMarker("Periapsis", 0, "Pe");
	}
	
	public void UpdateApoapsisMarker()
	{
		if (IsEscapeTrajectory) _orbitLineNode?.DeleteMarker("Apoapsis");
		else _orbitLineNode?.CreateMarker("Apoapsis", Math.PI, "Ap");
	}
	public void UpdatePeriapsisMarker()
	{
		if (IsEscapeTrajectory && GlobalValues.Time > T) _orbitLineNode.DeleteMarker("Periapsis");
		_orbitLineNode?.CreateMarker("Periapsis", 0, "Pe");
	}
	

	public void CreateMarkerAtTrueAnomaly(string labelName, double trueAnomaly, string labelText)
	{
		_orbitLineNode?.CreateMarker(labelName, trueAnomaly, labelText);
	}
	public void CreateMarkerAtTime(string labelName, double time, string labelText)
	{
		var trueAnomaly = TrueAnomalyFromTime(time);
		CreateMarkerAtTrueAnomaly(labelName, trueAnomaly, labelText);
	}

	public void UpdateMarkerFromTrueAnomaly(string labelName, double trueAnomaly, string labelText)
	{
		_orbitLineNode.UpdateMarker(labelName, trueAnomaly, labelText);
	}
	public void UpdateMarkerFromTime(string labelName, double time, string labelText)
	{
		var trueAnomaly = TrueAnomalyFromTime(time);
		UpdateMarkerFromTrueAnomaly(labelName, trueAnomaly, labelText);
	}

	public Vector3d PositionCurrent() => PositionFromTime(GlobalValues.Time, true);
	public Vector3d VelocityCurrent() => VelocityFromTime(GlobalValues.Time, true);

	public Vector3d PositionFromTime(double time, bool useAnomalyPrev = false) => PositionFromTrueAnomaly(TrueAnomalyFromTime(time, useAnomalyPrev));
	public Vector3d VelocityFromTime(double time, bool useAnomalyPrev = false) => VelocityFromTrueAnomaly(TrueAnomalyFromTime(time, useAnomalyPrev));
	
	public Vector3d PositionFromTrueAnomaly(double v)
	{
		var x = p * Math.Cos(v) / (1 + e * Math.Cos(v));
		var z = p * Math.Sin(v) / (1 + e * Math.Cos(v));

		return RotateToOrbitalPlane(x, z);
	}
	public Vector3d VelocityFromTrueAnomaly(double v)
	{
		var temp = Math.Sqrt(Primary.Mu / p);
			
		var x = -temp * Math.Sin(v);
		var z = temp * (e + Math.Cos(v));

		return RotateToOrbitalPlane(x, z);
	}

	private Vector3d RotateToOrbitalPlane(double x, double z)
	{
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
	
	public static implicit operator string(Orbit orbit)
	{
		return $"p:{orbit.p:F0}, e:{orbit.e:F3}, w:{orbit.w:F3}, i:{orbit.i:F3}, l:{orbit.l:F3}, n:{orbit.n:F10}, T:{orbit.T}, Parent:{orbit.Primary.Name}, ConicType: {orbit.OrbitType}";
	}
}
