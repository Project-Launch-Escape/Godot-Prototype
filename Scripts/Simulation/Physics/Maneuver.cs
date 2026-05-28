using Godot;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Maneuver
{
	public Trajectory ParentTrajectory;
	public int OrbitIndex;
	public ConicPatch ParentConic => ParentTrajectory.ConicPatches[OrbitIndex];
	public Orbit ParentOrbit => ParentConic.Orbit;
	public ManeuverMarker Marker;
	public double BurnTrueAnomaly;

	public Basis OrbitalBasis => ParentOrbit.OrbitalBasisFromTrueAnomaly(BurnTrueAnomaly);
	public double BurnTime => ParentOrbit.TimeFromTrueAnomaly(BurnTrueAnomaly);
	public Vector3d DeltaVOrbital; // +x is radial out, +y is normal, +z is prograde
	public Vector3d DeltaVAligned => (Vector3)DeltaVOrbital * OrbitalBasis.Inverse(); // Axes aligned with typical worldspace axes
	
	public Trajectory PostTrajectory = new () {IsManeuver = true};

	public Maneuver(Trajectory parentTrajectory, int orbitIndex, double burnTrueAnomaly, Vector3d deltaV)
	{
		ParentTrajectory = parentTrajectory;
		OrbitIndex = orbitIndex;
		BurnTrueAnomaly = burnTrueAnomaly;
		DeltaVOrbital = (Vector3)deltaV * OrbitalBasis;
		Marker = (ManeuverMarker)ParentConic.CreateMarkerOfType(OrbitMarkerType.Maneuver);
		Marker.Initialize(this);
	}

	public void CalculateTrajectory()
	{
		var startPosition = GetBurnStartPosition();
		var startVelocity = ParentOrbit.VelocityFromTrueAnomaly(BurnTrueAnomaly) + DeltaVAligned;
		
		PostTrajectory.SetFromStateVectors(startPosition, startVelocity, ParentOrbit.Primary, new Color(0.85f, 0.35f, 0.05f), BurnTime);
	}

	public Vector3d GetBurnStartPosition() => ParentOrbit.PositionFromTrueAnomaly(BurnTrueAnomaly);
}
