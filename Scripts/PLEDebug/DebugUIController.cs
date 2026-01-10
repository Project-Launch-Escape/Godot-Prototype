using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.PLEDebug;

public partial class DebugUIController : Control
{
	private static Dictionary<string,Label> _uiSections = new ();

	[Export] private Node _sectionContainerSetter;
	[Export] private PackedScene _sectionPrefabSetter;
	
	private static Node _sectionContainer;
	private static PackedScene _sectionPrefab;
	
	private static int _celestialIndex;
	
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
		CreateSection("PhysicsMode", "Physics Mode: Kepler");
	}
	
	public override void _Process(double delta)
	{
		UpdateFPS();
	}
	
	private static void UpdateFPS()
	{
		_uiSections["FPS"].Text = $"FPS: {Engine.GetFramesPerSecond():F1}";
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
