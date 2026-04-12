using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

public partial class OrbitMarker : HoverIcon
{
	protected static Camera3D Camera => GlobalValues.RenderSpaceCamera;

	private static readonly Dictionary<OrbitMarkerType, PackedScene> OrbitMarkerPrefabs = new ()
	{
		{ OrbitMarkerType.Apoapsis, GD.Load<PackedScene>("res://Scenes/UIElements/OrbitMarker.tscn")},
		{ OrbitMarkerType.Periapsis, GD.Load<PackedScene>("res://Scenes/UIElements/OrbitMarker.tscn")},
		{ OrbitMarkerType.Position, GD.Load<PackedScene>("res://Scenes/UIElements/CelestialMarker.tscn")}
	};

	public ConicPatch ParentConic => OrbitLine.Conic;
	public Orbit ParentOrbit => OrbitLine.Orbit;
	public OrbitMesh OrbitLine;
	public OrbitMarkerType MarkerType;
	protected Vector3d _renderspacePosition;

	public static OrbitMarker CreateMarker(OrbitMesh markedOrbitLine, OrbitMarkerType markerType)
	{
		var marker = (OrbitMarker)OrbitMarkerPrefabs[markerType].Instantiate();
		marker.OrbitLine = markedOrbitLine;
		marker.MarkerType = markerType;

		return marker;
	}
	
	protected virtual bool GetVisibility()
	{
		var behind = Camera.IsPositionBehind((Vector3)_renderspacePosition - Camera.GlobalPosition);
		return !behind && GlobalValues.UIVisible && OrbitLine.Visible;
	}

	public override void _Process(double delta)
	{
		if (!Visible) return;
		Position = Camera.UnprojectPosition((Vector3)_renderspacePosition);
	}


	protected string GetTimeString(double time)
	{
		time -= GlobalValues.Time;
		if (ParentOrbit.OrbitType is ConicType.Elliptical) time = Mathf.PosMod(time, ParentOrbit.Period);
		
		var timeString = "T-";
		
		var years = (long)(time / GlobalValues.Year);
		time -= years * GlobalValues.Year;
		if (years >= 1) timeString += $"{years}yr ";
		
		var days = (long)(time / GlobalValues.Day);
		time -= days * GlobalValues.Day;
		if (days >= 1) timeString += $"{days}d ";
		
		var hours = (long)(time / GlobalValues.Hour);
		time -= hours * GlobalValues.Hour;
		if (hours >= 1) timeString += $"{hours}hr ";
		
		var minutes = (long)(time / GlobalValues.Minute);
		time -= minutes * GlobalValues.Minute;
		if (minutes >= 1) timeString += $"{minutes}m ";
		
		var seconds = (long)time;
		timeString += $"{seconds}s";
		
		return timeString;
	}
}


public enum OrbitMarkerType
{
	Periapsis,
	Apoapsis,
	Position
}
