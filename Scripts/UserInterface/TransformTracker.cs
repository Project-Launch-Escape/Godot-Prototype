using Godot;

namespace GodotPrototype.Scripts.UserInterface;

public partial class TransformTracker : Control
{

	public static Camera3D Camera;
	[Export] private Label _transformName;

	private Node3D _toTrack;
	
	private static readonly float[] ScaleAtCoordLayer = [1f, 1.125f, 1f, 0.7f];
	
	public override void _Ready()
	{
		_toTrack = (Node3D)GetParent();
		_transformName.Text = _toTrack.Name;

		Scale = Vector2.One * ScaleAtCoordLayer[(int)((Celestial)_toTrack).NestedPos.CoordLayer];
	}

	public override void _Process(double delta)
	{
		var behind = Camera.IsPositionBehind(_toTrack.GlobalPosition);
		Visible = !behind && GlobalValues.UIVisible;
		if (behind) return;
		Position = Camera.UnprojectPosition(_toTrack.GlobalPosition);
	}
}
