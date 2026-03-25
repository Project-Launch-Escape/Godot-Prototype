using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface.UIElements;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

namespace GodotPrototype.Scripts.Simulation.Physics;

public class Trajectory
{
	public List<ConicPatch> ConicPatches = [];

	public Orbit CurrentOrbit
	{
		get => ConicPatches[0].Orbit;
		set => UpdatePatchAtIndex(0,value, new Range());
	}
	public Orbit FinalOrbit
	{
		get => ConicPatches[^1].Orbit;
		set => UpdatePatchAtIndex(ConicPatches.Count - 1,value, new Range());
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

	private void UpdatePatchAtIndex(int index, ConicPatch newPatch)
	{
		UpdatePatchAtIndex(index, newPatch.Orbit, newPatch.TimeRange);
	}
	private void UpdatePatchAtIndex(int index, Orbit newOrbit, Range newRange)
	{
		if (ConicPatches.Count >= index + 1)
		{
			var orbitToUpdate = ConicPatches[index].Orbit;
			orbitToUpdate.SetFromOrbit(newOrbit);
			ConicPatches[index].TimeRange = newRange;
			
			var trueAnomalyRange = ConicPatches[index].GetTrueAnomalyRange();

			orbitToUpdate.UpdateOrbitLine(trueAnomalyRange);
		}
		else
		{
			var newPatch = new ConicPatch(newOrbit, newRange);
			ConicPatches.Add(newPatch);
			var trueAnomalyRange = newPatch.GetTrueAnomalyRange();
			
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

		var trueAnomalyRange = ConicPatches[0].GetTrueAnomalyRange();
		CurrentOrbit.UpdateOrbitLine(trueAnomalyRange);
	}

	public Orbit GetOrbitAtTime(double time)
	{
		foreach (var conicPatch in ConicPatches)
		{
			if (conicPatch.TimeRange.ContainsValue(time)) return conicPatch.Orbit;
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
			var escape = CheckForEscapeTrajectory(i);
			var intercept = CheckForIntercept(i);
			
			var escapeTime = escape?.EncounterTime ?? double.MaxValue;
			var interceptTime = intercept?.EncounterTime ?? double.MaxValue;
			var encounter = escapeTime < interceptTime ? escape : intercept;
			
			if (encounter == null)
			{
				RemovePatch(i + 1);
				for (var j = 0; j < ConicPatches.Count; j++)
				{
					var patch = ConicPatches[j];
					patch.Orbit.UpdateOrbitLine(patch.GetTrueAnomalyRange());
				}
				return;
			}
			ConicPatches[i].TimeRange = new Range(ConicPatches[i].TimeRange.MinValue, encounter.EncounterTime);
			UpdatePatchAtIndex(i + 1, encounter.GetNewPatch());
		}

		
	}

	private ConicEncounter CheckForEscapeTrajectory(int indexToCheck)
	{
		var orbit = ConicPatches[indexToCheck].Orbit;
		
		if (!orbit.IsEscapeTrajectory || orbit.Primary.ParentCelestial == null) return null;
		
		var escapeTrueAnomaly = orbit.TrueAnomalyFromDistance(orbit.Primary.SOIRadius);
		var escapeTime = orbit.TimeFromTrueAnomaly(escapeTrueAnomaly);
		return new ConicEncounter(orbit.Primary.ParentCelestial, orbit, escapeTime);
	}
	private ConicEncounter CheckForIntercept(int indexToCheck)
	{
		var patch = ConicPatches[indexToCheck];
		var orbit = patch.Orbit;
		var timeRange = patch.TimeRange;
		
		var celestialsToCheck = GetRelevantCelestials(orbit);
		if (celestialsToCheck.Count <= 0) return null;
		
		List<double> encounterTimes = [];
		foreach (var celestial in celestialsToCheck)
		{
			var minTime = double.IsInfinity(timeRange.MinValue) ? GlobalValues.Time : timeRange.MinValue;
			var maxTime = double.IsInfinity(timeRange.MaxValue) ? minTime + 2 * orbit.Period : timeRange.MaxValue;
			var checkRange = new Range(minTime, maxTime);

			var encounterTime = FindEncounter2(orbit, celestial, checkRange);
			if (!double.IsNaN(encounterTime)) encounterTimes.Add(encounterTime);
		}
		if (encounterTimes.Count <= 0) return null;
		
		double earliestTime = double.MaxValue;
		Celestial earliestEncounterWith = null;
		for (int i = 0; i < encounterTimes.Count; i++)
		{
			var time = encounterTimes[i];
			if (time >= earliestTime) continue;

			earliestTime = time;
			earliestEncounterWith = celestialsToCheck[i];
		}

		return new ConicEncounter(earliestEncounterWith, orbit, earliestTime);
	}

	private static List<Celestial> GetRelevantCelestials(Orbit toCheck)
	{
		var relevantCelestials = new List<Celestial>();
		if (toCheck.Primary == null) return relevantCelestials;

		var vesselOrbitRange = toCheck.DistanceRange;
		
		foreach (var celestial in toCheck.Primary.ChildCelestials)
		{
			var rSOI = celestial.SOIRadius;
			var celestialOrbitRange = celestial.CelestialOrbit.DistanceRange;
			celestialOrbitRange = new Range(celestialOrbitRange.MinValue - rSOI, celestialOrbitRange.MinValue + rSOI);
			if (vesselOrbitRange.IntersectsRange(celestialOrbitRange))
			{
				relevantCelestials.Add(celestial);
			}
		}
		
		return relevantCelestials;
	}

	/// Returns double.NaN if no encounter is found
	public static double FindInterceptTime(Orbit vesselOrbit, Celestial celestial, Range timeRange)
	{
		/*
			Encounter Algorithm:
		0) Find time interval where an encounter could possibly occur
		1) Sample points along the given time interval
		2) Find the time with the lowest D(t)
		3) Use this time as a starting value for Newton's method to converge towards a zero of D(t).
			Once a point within the SOI boundary is chosen, stop the solver.This time is the 'within time'
		4) Choose a sample time left of the within time with positive D_2(t)
		5) Apply Bisection method with the sample time as the left bound and the within time as the right bound to attain the rough interval of the encounter time
		6) Apply Newton's method on D_2(t) to refine this estimation and get the final encounter time
		*/
		
		var celestialOrbit = celestial.CelestialOrbit;
		if (vesselOrbit.Primary != celestialOrbit.Primary)
		{
			return double.NaN;
		}
		double soiRadius = celestial.SOIRadius;
		
		// Step 1:
		const int nSamples = 24;
		var timeSamples = new double[nSamples];
		for (int i = 0; i < nSamples; i++)
		{
			timeSamples[i] = timeRange.LerpBetween((double)i/nSamples);
		}
		
		// Step 2:
		double minDist = 0;
		double minDistTime = timeRange.MinValue;
		foreach (var timeSample in timeSamples)
		{
			double newDist = DistanceAtTime(timeSample);
			if (DistanceAtTime(timeSample) >= minDist) continue;
			minDist = newDist;
			minDistTime = timeSample;
		}
		
		// Step 3:
		{
			int iter = 0;
			while (DistanceAtTime(minDistTime) >= soiRadius)
			{
				if (iter >= 8)
				{
					if (Engine.GetFramesDrawn() % 48 == 0) GD.Print("Encounter failed at Step 3");
					return double.NaN;
				}
				minDistTime -= 1.5 * (DistanceAtTime(minDistTime)-soiRadius) / DistDerivativeAtTime(minDistTime);
				iter++;
			}
		}
		if (DistanceAtTime(minDistTime) >= soiRadius || !timeRange.ContainsValue(minDistTime))
		{
			if (Engine.GetFramesDrawn() % 48 == 0) GD.Print("Encounter passed step 3 with an invalid time");
			return double.NaN;
		}

		// Step 4:
		double leftOfEncounterTime = timeRange.MinValue;
		for (int i = timeSamples.Length-1; i >= 0; i--)
		{
			double timeSample = timeSamples[i];
			if (timeSample >= minDistTime || DistanceAtTime(timeSample) < soiRadius) continue;
			
			leftOfEncounterTime = timeSample;
			break;
		}
		if (DistanceAtTime(leftOfEncounterTime) < soiRadius)
		{
			if (Engine.GetFramesDrawn() % 48 == 0) GD.Print("Encounter failed at step 4");
			return double.NaN;
		}
		
		// Step 5:
		var leftTime = leftOfEncounterTime;
		var rightTime = minDistTime;
		if (DistanceAtTime(leftTime) <= soiRadius || DistanceAtTime(rightTime) >= soiRadius)
		{
			GD.Print($"Preconditions for Bisection failed \tleft({DistanceAtTime(leftTime)}) right({DistanceAtTime(rightTime)})");
		}
		for (int iter = 0; iter < 8; iter++)
		{
			double midPointTime = (rightTime + leftTime) / 2;
			double midPointDist = DistanceAtTime(midPointTime);
			if (midPointDist < soiRadius) rightTime = midPointTime;
			else leftTime = midPointTime;
		}
		double encounterTime = (rightTime + leftTime) / 2;
		
		// Step 6:
		const double epsilon = 0.1;
		for (int iter = 0; iter < 12; iter++)
		{
			double dt = (DistanceAtTime(encounterTime) - soiRadius) / DistDerivativeAtTime(encounterTime);
			encounterTime -= dt;
			if (Math.Abs(dt) < epsilon) break;
		}
		if (!timeRange.ContainsValue(encounterTime))
		{
			if (Engine.GetFramesDrawn() % 48 == 0) GD.Print("Encounter failed to converge on interval");
			return double.NaN;
		}
		
		if (DistDerivativeAtTime(encounterTime) > 0)
		{
			if (Engine.GetFramesDrawn() % 48 == 0) GD.Print("Encounter converged on exit");
			return double.NaN;
		}
		
		return encounterTime;
		
		
		double DistanceAtTime(double time) => (celestialOrbit.PositionFromTime(time) - vesselOrbit.PositionFromTime(time)).Magnitude;
		double DistDerivativeAtTime(double time) =>
			(celestialOrbit.PositionFromTime(time) - vesselOrbit.PositionFromTime(time)).Normalized().Dot(celestialOrbit.VelocityFromTime(time) - vesselOrbit.VelocityFromTime(time));
	}

	public static double FindEncounter2(Orbit vesselOrbit, Celestial celestial, Range timeRange)
	{
		double time = timeRange.LerpBetween(0.5);
		var celestialOrbit = celestial.CelestialOrbit;
		int iterationCount = 0;
		bool hasEncounter = false;
		double dT = 0.5 * vesselOrbit.Period;
		
		double soiSquared = celestial.SOIRadius * celestial.SOIRadius;
		double minDistSquared = Math.Abs(DistanceSquaredAtTime(time));
		
		const int maxIterations = 48;
		const double epsilon = 1; // Minimum dt
		while (dT > epsilon && iterationCount < maxIterations)
		{
			var pastDistSquared = time + dT > timeRange.MaxValue ? double.MaxValue : DistanceSquaredAtTime(time + dT);
			var futureDistSquared = time - dT < timeRange.MinValue ? double.MaxValue : DistanceSquaredAtTime(time - dT);

			var absPastDist = Math.Abs(pastDistSquared);
			var absFutureDist = Math.Abs(futureDistSquared);
			
			if (!hasEncounter && (minDistSquared < 0.0 || pastDistSquared < 0.0 || futureDistSquared < 0.0)) hasEncounter = true; 
			
			if (absPastDist < minDistSquared && absPastDist < absFutureDist)
			{
				minDistSquared = absPastDist;
				time -= dT;
			}
			else if (absFutureDist < minDistSquared)
			{
				minDistSquared = absFutureDist;
				time += dT;
			}

			const double r = 0.5;
			dT *= r;
			iterationCount++;
		}
		return hasEncounter ? time : double.NaN;

		double DistanceSquaredAtTime(double time) => (vesselOrbit.PositionFromTime(time) - celestialOrbit.PositionFromTime(time)).MagnitudeSquared() - soiSquared;
	}

	public class ConicPatch
	{
		public Range TimeRange;
		public Orbit Orbit;

		public ConicPatch(Orbit orbit, Range timeRange)
		{
			TimeRange = timeRange;
			Orbit = orbit;
		}

		public Range? GetTrueAnomalyRange()
		{
			Range? trueAnomalyRange;
			if (double.IsInfinity(TimeRange.MaxValue))
			{
				trueAnomalyRange = null;
			}
			else
			{
				var vAtMin = Orbit.TrueAnomalyFromTime(Math.Max(GlobalValues.Time, TimeRange.MinValue));
				var vAtMax = Orbit.TrueAnomalyFromTime(TimeRange.MaxValue);
				trueAnomalyRange = vAtMax > vAtMin ? new Range(vAtMin, vAtMax) : new Range(vAtMax, vAtMin);
				if (trueAnomalyRange.Value.Width >= Math.Tau) trueAnomalyRange = null;
			}

			return trueAnomalyRange;
		}
	}

	public class ConicEncounter
	{
		public Orbit PreviousOrbit;
		public double EncounterTime;
		public Celestial NewPrimary;
		public EncounterType Type;

		public enum EncounterType
		{
			Escape,
			Intercept
		}

		public ConicEncounter(Celestial newPrimary, Orbit previousOrbit, double encounterTime)
		{
			EncounterTime = encounterTime;
			PreviousOrbit = previousOrbit;
			NewPrimary = newPrimary;
			Type = NewPrimary.PositionRel.CoordLayer < PreviousOrbit.Primary.PositionRel.CoordLayer ? EncounterType.Escape : EncounterType.Intercept;
		}

		public ConicPatch GetNewPatch()
		{
			var encounterTrueAnomaly = PreviousOrbit.TrueAnomalyFromTime(EncounterTime);

			Vector3d newPos;
			Vector3d newVelocity;
			var posAtEncounter = PreviousOrbit.PositionFromTrueAnomaly(encounterTrueAnomaly);
			var velAtEncounter = PreviousOrbit.VelocityFromTrueAnomaly(encounterTrueAnomaly);
			
			if (Type is EncounterType.Escape)
			{
				var parentPosAtEscape = PreviousOrbit.Primary.CelestialOrbit.PositionFromTime(EncounterTime);
				var parentVelAtEscape = PreviousOrbit.Primary.CelestialOrbit.VelocityFromTime(EncounterTime);

				newPos = parentPosAtEscape + posAtEncounter;
				newVelocity = velAtEncounter + parentVelAtEscape;
			}
			else
			{
				var interceptedPosAtEncounter = NewPrimary.CelestialOrbit.PositionFromTime(EncounterTime);
				var interceptedVelAtEncounter = NewPrimary.CelestialOrbit.VelocityFromTime(EncounterTime);
				newPos = posAtEncounter - interceptedPosAtEncounter;
				newVelocity = velAtEncounter - interceptedVelAtEncounter;
			}
		
			var newOrbit = new Orbit(newPos, newVelocity, NewPrimary, PreviousOrbit.Color, EncounterTime);
			var newTimeRange = new Range(EncounterTime);
			return new ConicPatch(newOrbit, newTimeRange);
		}
	}
}
