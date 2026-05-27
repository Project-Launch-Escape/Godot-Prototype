using Godot;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Maneuver
{
    public ConicPatch ParentConic;
    public Orbit ParentOrbit => ParentConic.Orbit;
    public ManeuverMarker Marker;
    public double BurnTrueAnomaly;
    public double BurnTime => ParentOrbit.TimeFromTrueAnomaly(BurnTrueAnomaly);
    public Vector3d DeltaV;
    
    public Trajectory PostTrajectory = new () {IsManeuver = true};

    public Maneuver(ConicPatch parentConic, double burnTrueAnomaly, Vector3d deltaV)
    {
        ParentConic = parentConic;
        BurnTrueAnomaly = burnTrueAnomaly;
        DeltaV = deltaV;
        Marker = (ManeuverMarker)ParentConic.CreateMarkerOfType(OrbitMarkerType.Maneuver);
        Marker.ParentManeuver = this;
    }

    public void CalculateTrajectory()
    {
        var startPosition = GetBurnStartPosition();
        var startVelocity = ParentOrbit.VelocityFromTrueAnomaly(BurnTrueAnomaly) + DeltaV;
        
        PostTrajectory.SetFromStateVectors(startPosition, startVelocity, ParentOrbit.Primary, new Color(0.85f, 0.35f, 0.05f), BurnTime);
    }

    public Vector3d GetBurnStartPosition() => ParentOrbit.PositionFromTrueAnomaly(BurnTrueAnomaly);
}