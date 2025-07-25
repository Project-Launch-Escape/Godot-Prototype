using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.Debug;

public partial class DebugUIController : Control
{
	private static Dictionary<string,Label> _uiSections = new ();

	[Export] private Node _sectionContainerSetter;
	[Export] private PackedScene _sectionPrefabSetter;
	
	private static Node _sectionContainer;
	private static PackedScene _sectionPrefab;
	
	private static int _celestialIndex;
	private static float[] _fpsPrev = new float[120];
	private static int _frame;
	
	public override void _Ready()
	{
		_sectionContainer = _sectionContainerSetter;
		_sectionPrefab = _sectionPrefabSetter;
		CreateDefaultSections();
	}

	public static void CreateSection(string name, string labelText = "")
	{
		var newSection = _sectionPrefab.Instantiate();
		var sectionLabel = (Label)newSection;
		
		newSection.Name = name;
		sectionLabel.Text = labelText;
		
		_uiSections.Add(name, sectionLabel);
		_sectionContainer.AddChild(newSection);
	}

	public static void UpdateSection(string toUpdate, string valueText)
	{
		_uiSections[toUpdate].Text = valueText;
	}

	private static void CreateDefaultSections()
	{
		CreateSection("FPS", "FPS:");
		CreateSection("CurrentTime", "Current Time:");
		CreateSection("Paused", "Paused");
		CreateSection("TimeScale", "Time Scale:");
		CreateSection("Speed", "Camera Speed:");
		CreateSection("SOI", "Current SOI:");
		CreateSection("CelestialDist", "Distance To Celestial:");
		CreateSection("PhysicsMode", "Physics Mode: Kepler");
	}
	
	public override void _Process(double delta)
	{
		UpdateFPS(delta);
		UpdatePausedText();
		UpdateCurrentTime(GlobalValues.Time);
		UpdateTimeScale(GlobalValues.TimeScale);
		UpdateSpeed(FlightCamera.Speed);
		UpdateDistanceTo(Celestial.AllCelestials[_celestialIndex]);
		//UpdatePhysicsMode(Vessel.ActiveVessel.PhysicsMode);
	}
	
	private static void UpdateFPS(double delta)
	{
		_fpsPrev[_frame] = (float)(1 / delta);
		_uiSections["FPS"].Text = $"FPS: {_fpsPrev.Average():F1}";
		_frame = (_frame + 1) % _fpsPrev.Length;
	}

	private static void UpdatePausedText()
	{
		_uiSections["Paused"].Text = $"Paused: {GlobalValues.Paused}";
	}

	private static void UpdateCurrentTime(double time)
	{
		_uiSections["CurrentTime"].Text = "Current Time: " + GlobalValues.TimeToYearDayString(time);
	}
	
	private static void UpdateTimeScale(double scale)
	{
		_uiSections["TimeScale"].Text = scale switch
		{
			< GlobalValues.Minute => $"Current Rate: {scale:F1} s/s",
			< GlobalValues.Hour => $"Current Rate: {scale / GlobalValues.Minute:F1} min/s",
			< GlobalValues.Day => $"Current Rate: {scale / GlobalValues.Hour:F1} hr/s",
			< GlobalValues.Year => $"Current Rate: {scale / GlobalValues.Day:F1} day/s",
			_ => $"Current Rate: {scale / GlobalValues.Year:F1} yr/s"
		};
	}
	
	private static void UpdateSpeed(double speed)
	{
		_uiSections["Speed"].Text = $"Current Speed: {speed/1000:F2} km/s";
	}
	
	public static void UpdateSOI(Celestial currentSOI)
	{
		_uiSections["SOI"].Text = "Current SOI: " + currentSOI?.Name;
	}

	private static void UpdateDistanceTo(Celestial targetCelestial)
	{
		var dist = targetCelestial.RelPosition[CoordinateSpace.RenderSpace].Magnitude;
		_uiSections["CelestialDist"].Text = $"Distance to {targetCelestial.Name}: {dist/1000:F4}km";
	}
	public static void UpdatePhysicsMode(PhysicsType phystype)
	{
		var text = phystype is PhysicsType.Newton ? "Newton" : "Kepler";
		_uiSections["PhysicsMode"].Text = "Physics Mode: " + text;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Minus && _celestialIndex > 0)
			{
				_celestialIndex--;
				Mathf.PosMod(_celestialIndex, Celestial.AllCelestials.Count);
			}
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Equal)
			{
				_celestialIndex++;
				_celestialIndex %= Celestial.AllCelestials.Count;
			}
		}
	}
}
