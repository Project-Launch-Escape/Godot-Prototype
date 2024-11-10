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
				SelectPart(0);
				break;
			case InputEventKey { Keycode: Key.Key2, Pressed: false }:
				SelectPart(1);
				break;
			case InputEventKey { Keycode: Key.Key3, Pressed: false }:
				SelectPart(2);
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
	private void SelectPart(int n)
	{
		DeselectPart();
		
		_state = State.PlacingPart;
		switch (n) {
			case 0:
				_currentPlacingPart = (Node3D)GD.Load<PackedScene>("res://UnknownPi/capsule.tscn").Instantiate();break;
			
			case 1:
				_currentPlacingPart = (Node3D)GD.Load<PackedScene>("res://UnknownPi/tank.tscn").Instantiate();break;
			
			case 2:
				_currentPlacingPart = (Node3D)GD.Load<PackedScene>("res://UnknownPi/engine.tscn").Instantiate();break;
			
		}
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
