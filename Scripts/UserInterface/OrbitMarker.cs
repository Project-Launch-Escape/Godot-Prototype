using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface;

public partial class OrbitMarker : Control
{
	public static Camera3D Camera => GlobalValues.RenderSpaceCamera;
	private static readonly PackedScene OrbitMarkerPrefab = GD.Load<PackedScene>("res://Prefabs/orbit_marker.tscn");

	public RelativePosition RelPosition;
	[Export] public Label TextBox;

	public static OrbitMarker CreateMarker(RelativePosition position, string markerText)
	{
		var markerNode = OrbitMarkerPrefab.Instantiate();
		var markerScript = (OrbitMarker)markerNode;
		
		markerScript.RelPosition = position;
		markerScript.TextBox.Text = markerText;
		
		return (OrbitMarker)markerNode;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var renderSpacePosition = (Vector3)RelPosition[CoordinateSpace.RenderSpace];
		
		var behind = Camera.IsPositionBehind(renderSpacePosition);
		Visible = !behind && GlobalValues.UIVisible;
		if (behind) return;
		Position = Camera.UnprojectPosition(renderSpacePosition);
	}
}
