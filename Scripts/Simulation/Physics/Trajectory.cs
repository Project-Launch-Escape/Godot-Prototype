using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface.UIElements;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Trajectory
{
	public List<(Orbit Orbit, Range TimeRange)> ConicPatches = [];

	public Orbit CurrentOrbit
	{
		get => ConicPatches[0].Orbit;
		set => UpdateOrbitAtIndex(0,value, new Range());
	} 
	public Orbit FinalOrbit
	{
		get => ConicPatches[^1].Orbit;
		set => UpdateOrbitAtIndex(ConicPatches.Count - 1,value, new Range());
	} 
	private const byte MaxDepth = 10;
	

	public Trajectory()
	{

	}

	public Trajectory(Orbit conicPatch)
	{
		CurrentOrbit = conicPatch;
		FindEncounters();
	}

	public Trajectory(Vector3d position, Vector3d velocity, Celestial parentCelestial, Color color)
	{
		var orbit = new Orbit(position, velocity, parentCelestial, color);
		CurrentOrbit = orbit;
		
		FindEncounters();
	}

	private void UpdateOrbitAtIndex(int index, Orbit newOrbit, Range newRange)
	{
		if (ConicPatches.Count >= index + 1)
		{
			var orbit = ConicPatches[index].Orbit;
			orbit.SetFromOrbit(newOrbit);
			ConicPatches[index] = (orbit, newRange);

			Range? trueAnomalyRange = orbit.IsEscapeTrajectory
				? Range.Intersect(new Range(orbit.TrueAnomalyFromTime(GlobalValues.Time)), orbit.TrueAnomalyRange) : null;
			
			orbit.UpdateOrbitLine(trueAnomalyRange);
		}
		else
		{
			ConicPatches.Add((newOrbit, newRange));
			
			Range? trueAnomalyRange = newOrbit.IsEscapeTrajectory
				? Range.Intersect(new Range(newOrbit.TrueAnomalyFromTime(GlobalValues.Time)), newOrbit.TrueAnomalyRange) : null;
			
			newOrbit.CreateOrbitLine(trueAnomalyRange);
			newOrbit.CreateMarkerOfType(OrbitMarkerType.Apoapsis);
			newOrbit.CreateMarkerOfType(OrbitMarkerType.Periapsis);
		}

	}

	private void RemovePatch(int index)
	{
		for (int i = ConicPatches.Count - 1; i >= index; i--)
		{
			ConicPatches[i].Orbit.DeleteOrbitLine();
			ConicPatches.RemoveAt(i);
		}
	}

	public void SetFromStateVectors(Vector3d position, Vector3d velocity, Celestial parentCelestial)
	{
		CurrentOrbit = new Orbit(position, velocity, parentCelestial, CurrentOrbit.Color);
		FindEncounters();
	}

	public void SetFromPrimaryOrbit(Orbit primary)
	{
		CurrentOrbit = primary;
		FindEncounters();
	}

	public void Update()
	{
		if (GetOrbitAtTime(GlobalValues.Time) != CurrentOrbit)
		{
			ConicPatches[0].Orbit.DeleteOrbitLine();
			ConicPatches.RemoveAt(0);
		}

		if (!CurrentOrbit.IsEscapeTrajectory) return;
		
		Range? trueAnomalyRange = CurrentOrbit.IsEscapeTrajectory
			? Range.Intersect(new Range(CurrentOrbit.TrueAnomalyFromTime(GlobalValues.Time)), CurrentOrbit.TrueAnomalyRange) : null;
		CurrentOrbit.UpdateOrbitLine(trueAnomalyRange);
	}

	public Orbit GetOrbitAtTime(double time)
	{
		for (int i = 0; i < ConicPatches.Count; i++)
		{
			var timeRange = ConicPatches[i].TimeRange;

			if (timeRange.ContainsValue(time)) return ConicPatches[i].Orbit;
		}

		throw new IndexOutOfRangeException();
	}

	public RelativePosition PositionFromTime(double time)
	{
		var orbit = GetOrbitAtTime(time);
		return new RelativePosition(orbit.PositionFromTime(time), orbit.Primary);
	}

	public RelativeVelocity VelocityFromTime(double time)
	{
		var orbit = GetOrbitAtTime(time);
		return new RelativeVelocity(orbit.VelocityFromTime(time), orbit.Primary);
	}
	
	public RelativePosition PositionCurrent() => new (CurrentOrbit.PositionCurrent(), CurrentOrbit.Primary);
	public RelativeVelocity VelocityCurrent() => new (CurrentOrbit.VelocityCurrent(), CurrentOrbit.Primary);
	
	
	private void FindEncounters()
	{
		for (int i = 0; i < MaxDepth; i++)
		{
			if (!CheckForEscapeTrajectory(i))
			{
				RemovePatch(i + 1);
				return;
			}
		}
	}

	private bool CheckForEscapeTrajectory(int indexToCheck)
	{
		var orbit = ConicPatches[indexToCheck].Orbit;
		
		if (!orbit.IsEscapeTrajectory || orbit.Primary.ParentCelestial == null) return false;
		
		var escapeTrueAnomaly = orbit.TrueAnomalyFromDistance(orbit.Primary.SOIRadius);
		var escapeTime = orbit.TimeFromTrueAnomaly(escapeTrueAnomaly);

		var escapePos = orbit.PositionFromTrueAnomaly(escapeTrueAnomaly);
		var escapeVelocity = orbit.VelocityFromTrueAnomaly(escapeTrueAnomaly);

		var newParent = orbit.Primary.ParentCelestial;
		var parentPosAtEscape = orbit.Primary.CelestialOrbit.PositionFromTime(escapeTime);
		
		var newPos = parentPosAtEscape + escapePos;
		var newVelocity = escapeVelocity + orbit.Primary.CelestialOrbit.VelocityFromTime(escapeTime);

		var newOrbit = new Orbit(newPos, newVelocity, newParent, CurrentOrbit.Color, escapeTime);
		var newTimeRange = new Range(escapeTime);

		ConicPatches[indexToCheck] = (ConicPatches[indexToCheck].Orbit, new Range(ConicPatches[indexToCheck].TimeRange.MinValue, escapeTime));
		UpdateOrbitAtIndex(indexToCheck + 1, newOrbit, newTimeRange);
		return true;
	}
	
	//TODO: Add encounter detection
	/*
	
	private void CheckForEncounter(Celestial celestials)
	{
		
	}


	private List<Celestial> GetRelevantCelestials(Orbit toCheck)
	{
		var relevantCelestials = new List<Celestial>();
		if (toCheck.Primary == null) return relevantCelestials;

		var vesselOrbitRange = toCheck.DistanceRange;
		
		foreach (var celestial in toCheck.Primary.ChildCelestials)
		{
			var celestialOrbitRange = celestial.CelestialOrbit.DistanceRange;
			if (vesselOrbitRange.IntersectsRange(celestialOrbitRange))
			{
				relevantCelestials.Add(celestial);
			}
		}
		
		foreach (var celestial in relevantCelestials)
		{
			if (FindClosestPossibleApproach(toCheck, celestial.CelestialOrbit) <= celestial.SOIRadius)
			{
				
			}
		}
		
		return relevantCelestials;
	}

	
	private double FindClosestPossibleApproach(Orbit orbit1, Orbit orbit2)
	{
		var K = orbit1.NormalVector.Cross(orbit2.NormalVector);
		return 1;
	}
	

	private double FindEncounterTime(Orbit vesselOrbit, Orbit celestialOrbit, double encounterDist)
	{
		var encounterTime = GlobalValues.Time;
		const double tolerance = 1E-4;
		// Solve for encounter time using Newtons Method
		
		for (int i = 0; i < 10; i++)
		{
			var posRelative = vesselOrbit.PositionFromTime(encounterTime) - celestialOrbit.PositionFromTime(encounterTime);
			
			var y = posRelative.Magnitude - encounterDist;
			var yDerivative = (vesselOrbit.VelocityFromTime(encounterTime) - celestialOrbit.VelocityFromTime(encounterTime)).Dot(posRelative) / posRelative.Magnitude;
			
			var dt = y / yDerivative;
			encounterTime -= dt;
			if (dt < tolerance) break;
		}
		return encounterTime;
	}
	*/
}
