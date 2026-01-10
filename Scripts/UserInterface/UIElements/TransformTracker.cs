using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class TransformTracker : Control
{
	private Camera3D _camera;
	[Export] private Label _transformName;

	private Node3D _toTrack;
	private CoordinateSpace _coordSpace;
	
	private static readonly float[] ScaleAtCoordLayer = [1.125f, 1f, 0.7f];
	
	
	public override void _Ready()
	{
		_camera = GetViewport().GetCamera3D();
		_toTrack = (Node3D)GetParent();
		_transformName.Text = _toTrack.Name;
		_coordSpace = _camera == GlobalValues.RenderSpaceCamera ? CoordinateSpace.RenderSpace : CoordinateSpace.VesselSpace;
		
		if (_toTrack is Celestial celestial)
		{
			Scale = Vector2.One * ScaleAtCoordLayer[(int)celestial.PositionRel.CoordLayer];
		}
	}

	public override void _Process(double delta)
	{
		var behind = _camera.IsPositionBehind(_toTrack.GlobalPosition);
		Visible = !behind && GlobalValues.UIVisible;
		if (behind) return;
		Position = _camera.UnprojectPosition(_toTrack.GlobalPosition);
	}
}
