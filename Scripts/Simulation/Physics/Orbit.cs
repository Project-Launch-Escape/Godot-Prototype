using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.UserInterface.UIElements;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;
using GodotPrototype.Scripts.Vessels;
using OrbitMesh = GodotPrototype.Scripts.UserInterface.UIElements.OrbitMesh;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Orbit : OrbitalElements
{
	public Celestial Primary;
	public Color Color;

	public OrbitMesh OrbitLineNode;
	public IOrbiter OrbitingObject;

	public bool IsEscapeTrajectory => FindIfIsEscapeTrajectory();
	public Range TrueAnomalyRange => GetTrueAnomalyRange(); // True Anomaly range assuming infinite SOI
	public Range ConstrainedTrueAnomalyRange => GetTrueAnomalyRange(); // True Anomaly range assuming constrained SOI


	public Orbit()
	{
		
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
		OrbitingObject = orbit.OrbitingObject;
		Color = orbit.Color;
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
		if (OrbitLineNode != null) return;
		OrbitLineNode = OrbitMesh.CreateOrbitLine(this, trueAnomalyRange);
	}
	public void DeleteOrbitLine()
	{
		OrbitLineNode?.DeleteOrbitLine();
	}
	public void UpdateOrbitLine(Range? trueAnomalyRange = null)
	{
		OrbitLineNode?.UpdateOrbitLine(this, trueAnomalyRange);	
	}

	public OrbitMarker CreateMarkerOfType(OrbitMarkerType markerType)
	{
		return OrbitLineNode?.CreateMarkerOfType(markerType);
	}

	public Vector3d PositionCurrent() => PositionFromTime(GlobalValues.Time);
	public Vector3d VelocityCurrent() => VelocityFromTime(GlobalValues.Time);

	public Vector3d PositionFromTime(double time) => PositionFromTrueAnomaly(TrueAnomalyFromTime(time));
	public Vector3d VelocityFromTime(double time) => VelocityFromTrueAnomaly(TrueAnomalyFromTime(time));
	public Vector3d AccelerationFromTime(double time) => AccelerationFromTrueAnomaly(TrueAnomalyFromTime(time));
	
	public Vector3d PositionFromTrueAnomaly(double v)
	{
		if (OrbitType is ConicType.Static)
		{
			return OrbitingObject switch
			{
				Celestial celestial => celestial.PositionRel.LocalPosition,
				Vessel vessel => vessel.PositionRel.LocalPosition,
				_ => Vector3d.Zero
			};
		}
		
		var x = p * Math.Cos(v) / (1 + e * Math.Cos(v));
		var z = p * Math.Sin(v) / (1 + e * Math.Cos(v));

		return RotateToOrbitalPlane(x, z);
	}
	public Vector3d VelocityFromTrueAnomaly(double v)
	{
		if (OrbitType is ConicType.Static) return Vector3d.Zero;
		
		var temp = Math.Sqrt(Primary.Mu / p);
			
		var x = -temp * Math.Sin(v);
		var z = temp * (e + Math.Cos(v));

		return RotateToOrbitalPlane(x, z);
	}

	public Vector3d AccelerationFromTrueAnomaly(double v)
	{
		double r = p / (1 + e * Math.Cos(v));
		double temp = Primary.Mu / (r * r);

		double x = temp * -Math.Cos(v);
		double z = temp * -Math.Sin(v);
		return RotateToOrbitalPlane(x, z);
	}

	public Basis OrbitalBasisFromTrueAnomaly(double v)
	{
		var zVector = (Vector3)VelocityFromTrueAnomaly(v).Normalized();
		var yVector = (Vector3)NormalVector;
		var xVector = yVector.Cross(zVector);
		return new Basis(xVector, yVector, zVector);
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
