using Godot;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Maneuver
{
	public Trajectory ParentTrajectory;
	
	public int OrbitIndex;
	public ConicPatch ParentConic => ParentTrajectory.ConicPatches[OrbitIndex];
	public Orbit ParentOrbit => ParentConic.Orbit;
	public ManeuverMarker Marker;
	public double BurnTrueAnomaly;

	public bool Locked;
	private Vector3d LockedBurnStartPosition;
	private Vector3d LockedBurnStartVelocity;
	private Basis LockedOrbitalBasis;

	public Basis OrbitalBasis => Locked ? LockedOrbitalBasis : ParentOrbit.OrbitalBasisFromTrueAnomaly(BurnTrueAnomaly);
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
		if (Locked) return;
		var startPosition = GetBurnStartPosition();
		var startVelocity = GetBurnStartVelocity() + DeltaVAligned;
		
		PostTrajectory.SetFromStateVectors(startPosition, startVelocity, ParentOrbit.Primary, new Color(0.85f, 0.35f, 0.05f), BurnTime);
	}

	public Vector3d GetBurnStartPosition() => Locked ? LockedBurnStartPosition : ParentOrbit.PositionFromTrueAnomaly(BurnTrueAnomaly);
	public Vector3d GetBurnStartVelocity() => Locked ? LockedBurnStartVelocity : ParentOrbit.VelocityFromTrueAnomaly(BurnTrueAnomaly);

	public void DeleteManeuver()
	{
		int iThis = Vessel.ActiveVessel.Maneuvers.IndexOf(this);
		// Deletes this maneuver and the next Maneuver in the list. Ensures that maneuvers that depend on this one will also be deleted
		Vessel.ActiveVessel.Maneuvers.RemoveAt(iThis);
		if (iThis < Vessel.ActiveVessel.Maneuvers.Count) Vessel.ActiveVessel.Maneuvers[iThis].DeleteManeuver();
		
		Marker.EditorNode.QueueFree();
		ParentConic.DeleteMarkerOfType(OrbitMarkerType.Maneuver);
		PostTrajectory?.Delete();
	}

	public void ChangeLockMode(bool enabled)
	{
		if (Locked == enabled) return;
		if (enabled)
		{
			LockedBurnStartPosition = GetBurnStartPosition();
			LockedBurnStartVelocity = GetBurnStartVelocity();
			LockedOrbitalBasis = OrbitalBasis;
		}
		else
		{
			LockedBurnStartPosition = null;
			LockedBurnStartVelocity = null;
			LockedOrbitalBasis = Basis.Identity;
		}
		Locked = enabled;
	}
}
