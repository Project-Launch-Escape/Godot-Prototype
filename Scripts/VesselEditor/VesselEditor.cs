using Godot;
using GodotPrototype.Scripts.VesselEditor.Parts;

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

	[Export]
	public PartDefinition[] PartDefinitions;

	private State _state = State.Idle;
	private Vector3 _currentOrigin = default;

	private PartDefinition _currentPlacingPart = null;
	private Node3D _currentPlacingPartNode = null;
	
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
		
		_currentPlacingPartNode.Position = rayIntersection.Value;
	}

	// TODO: All of this is temporary for now
	private void SelectPart(int n)
	{
		DeselectPart();

		if (n < 0 || n >= PartDefinitions.Length)
			return;
		
		_state = State.PlacingPart;
		
		_currentPlacingPart = PartDefinitions[n];
		_currentPlacingPartNode = _currentPlacingPart.Asset.Instantiate<Node3D>();
		
		AddChild(_currentPlacingPartNode);
	}

	private void DeselectPart()
	{
		_currentPlacingPartNode?.QueueFree();
		
		_currentPlacingPartNode = null;
		_currentPlacingPart = null;

		_state = State.Idle;
	}

	private void PlacePart()
	{
		_currentPlacingPartNode = null;
		_currentPlacingPart = null;
		_state = State.Idle;
	}
}
