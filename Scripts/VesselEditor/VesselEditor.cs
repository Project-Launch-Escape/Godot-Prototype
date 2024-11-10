using Godot;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class VesselEditor : Node3D
{
	public enum State
	{
		Idle,
		PlacingPart
	}

	[Export]
	public Camera3D Camera;

	private State _state = State.Idle;
	private Vector3 _currentOrigin = default;

	private Node3D _currentPlacingPart = null;
	
	public override void _Process(double delta)
	{
		switch (_state)
		{
			case State.PlacingPart:
				ProcessPlacingPartState();
				break;
		}
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventKey { Keycode: Key.Key1, Pressed: false }:
				SelectPart();
				break;
			case InputEventKey { Keycode: Key.Escape, Pressed: false }:
				DeselectPart();
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } when _state == State.PlacingPart:
				PlacePart();
				break;
		}
	}

	private void ProcessPlacingPartState()
	{
		var cameraPosition = Camera.Position;
		var cameraPositionWithoutY = Camera.Position with { Y = 0 };
		
		var planeNormal = -(cameraPositionWithoutY - _currentOrigin).Normalized();
		var plane = new Plane(planeNormal, _currentOrigin);

		var rayIntersection = plane.IntersectsRay(cameraPosition, Camera.ProjectRayNormal(GetViewport().GetMousePosition()));

		if (rayIntersection == null)
			return;
		
		_currentPlacingPart.Position = rayIntersection.Value;
	}

	// TODO: All of this is temporary for now
	private void SelectPart()
	{
		DeselectPart();
		
		_state = State.PlacingPart;
		_currentPlacingPart = new CsgBox3D();
		
		AddChild(_currentPlacingPart);
	}

	private void DeselectPart()
	{
		_currentPlacingPart?.QueueFree();
		_currentPlacingPart = null;

		_state = State.Idle;
	}

	private void PlacePart()
	{
		_currentPlacingPart = null;
		_state = State.Idle;
	}
}
