using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.UserInterface.UIElements;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class ConicPatch
{
	public Range TimeRange;
	public Orbit Orbit;
	public OrbitMesh OrbitLineNode;

	public ConicPatch(Orbit orbit, Range timeRange)
	{
		TimeRange = timeRange;
		Orbit = orbit;
	}
		
	/// Returns null if the range is -pi to pi
	public Range GetTrueAnomalyRange()
	{
		Range trueAnomalyRange;
		var constrainedRange = new Range(Math.Max(GlobalValues.Time, TimeRange.MinValue), TimeRange.MaxValue);
		if (constrainedRange.IsInfinite || (Orbit.OrbitType is ConicType.Circular or ConicType.Elliptical && constrainedRange.Width >= Orbit.Period))
		{
			trueAnomalyRange = Orbit.TrueAnomalyRange;
		}
		else
		{
			var vAtMin = Orbit.TrueAnomalyFromTime(constrainedRange.MinValue);
			var vAtMax = Orbit.TrueAnomalyFromTime(constrainedRange.MaxValue);
			trueAnomalyRange = vAtMax > vAtMin ? new Range(vAtMin, vAtMax) : new Range(vAtMax, vAtMin);
			if (trueAnomalyRange.Width >= Math.Tau) trueAnomalyRange = Range.FullCirle;
		}
		return trueAnomalyRange;
	}
	
	public void CreateOrbitLine(bool isManeuver = false)
	{
		if (OrbitLineNode != null) return;
		OrbitLineNode = OrbitMesh.CreateOrbitLine(this, GetTrueAnomalyRange(), isManeuver);
	}
	public void DeleteOrbitLine()
	{
		OrbitLineNode?.DeleteOrbitLine();
	}
	public void UpdateOrbitLine(bool isManeuver = false)
	{
		OrbitLineNode?.UpdateOrbitLine(this, isManeuver);
	}

	public OrbitMarker CreateMarkerOfType(OrbitMarkerType markerType)
	{
		return OrbitLineNode?.CreateMarkerOfType(markerType);
	}

	public void DeleteMarkerOfType(OrbitMarkerType markerType)
	{
		OrbitLineNode.DeleteMarker(markerType);
	}
	
	public static implicit operator string(ConicPatch conic) => $"Orbit of type {conic.Orbit.OrbitType} valid between ({conic.TimeRange}). {(conic.OrbitLineNode is null ? "Does not have an OrbitLineNode" : "Has an OrbitLineNode")}";
}
