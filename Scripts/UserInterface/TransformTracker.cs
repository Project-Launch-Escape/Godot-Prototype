using Godot;

namespace GodotPrototype.Scripts.UserInterface;

public partial class TransformTracker : Control
{

	private Camera3D _camera;
	private Label _transformName;

	private bool _visible = true;
		
	private Node3D _toTrack;
	
	private static readonly float[] ScaleAtCoordLayer = [0.01f, 1.125f, 1f, 0.7f];
	
	public override void _Ready()
	{
		_camera = (Camera3D)GetNode("/root/GameScene/Camera");
		_transformName = (Label)FindChild("Label");
		
		_toTrack = (Node3D)GetParent();
		_transformName.Text = _toTrack.Name;

		Scale = new Vector2(1f,1f) * ScaleAtCoordLayer[(int)((CelestialScript)_toTrack).NestedPos.CoordLayer];
	}

	public override void _Process(double delta)
	{
		var behind = _camera.IsPositionBehind(_toTrack.GlobalPosition);
		Visible = !behind && _visible;
		if (behind) return;
		Position = _camera.UnprojectPosition(_toTrack.GlobalPosition);
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.M)
			{
				_visible = !_visible;
			}
		}
	}
}
