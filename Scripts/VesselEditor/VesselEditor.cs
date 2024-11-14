using System.Collections.Generic;
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

	private readonly List<VesselPart> _vesselParts = new();
	private readonly Dictionary<VesselPart, Node3D> _vesselPartNodes = new();
	
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
		var partPosition = GetPartPosition();
		
		if(partPosition != null)
			_currentPlacingPartNode.Position = partPosition!.Value;
	}

	// TODO: This needs refactoring
	private Vector3? GetPartPosition()
	{
		var basePartPosition = GetPartBasePosition();

		if (basePartPosition == null)
			return null;

		var globalTransform = _currentPlacingPartNode.GlobalTransform;
		globalTransform.Origin = basePartPosition.Value;
		
		foreach (var placingPartSnapPoint in _currentPlacingPart.SnapPoints)
		{
			var selfWorldPosition = globalTransform * placingPartSnapPoint.Position;
			var selfWorldForward = globalTransform * (placingPartSnapPoint.Orientation * Vector3.Forward);

			foreach (var vesselPart in _vesselParts)
			{
				var otherPartNode = _vesselPartNodes[vesselPart];
				var otherPartGlobalTransform = otherPartNode.GlobalTransform;

				foreach (var vesselPartSnapPoint in vesselPart.Part.SnapPoints)
				{
					var otherWorldPosition = otherPartGlobalTransform * vesselPartSnapPoint.Position;
					var otherWorldForward = otherPartGlobalTransform * (vesselPartSnapPoint.Orientation * Vector3.Forward);
					var correctionVector = selfWorldPosition - otherWorldPosition;
					
					if(correctionVector.Length() > 0.5f || otherWorldForward.Dot(selfWorldForward) < 0)
						continue;
					
					var newPartPos = basePartPosition.Value - correctionVector;
					return newPartPos;
				}
			}
		}
		
		return basePartPosition;
	}

	// TODO: This also needs refactoring
	private Vector3? GetPartBasePosition()
	{
		var cameraPosition = Camera.GlobalPosition;
		var cameraPositionWithoutY = Camera.GlobalPosition with { Y = 0 };
		
		var rayForward = Camera.ProjectRayNormal(GetViewport().GetMousePosition());

		var physicsSpaceState = GetWorld3D().DirectSpaceState;
		var partRid = ((CollisionObject3D)_currentPlacingPartNode).GetRid();
		
		var rayResult = physicsSpaceState.IntersectRay(new PhysicsRayQueryParameters3D
		{
			From = cameraPosition,
			To = rayForward * 100,
			Exclude = new Godot.Collections.Array<Rid>(new []{partRid})
		});

		if (rayResult.Count > 0)
		{
			return (Vector3) rayResult["position"];
		}
		
		var planeNormal = -(cameraPositionWithoutY - _currentOrigin).Normalized();
		var plane = new Plane(planeNormal, _currentOrigin);
		
		var rayIntersection = plane.IntersectsRay(cameraPosition, rayForward);
		return rayIntersection;
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
		var vesselPart = new VesselPart
		{
			Part = _currentPlacingPart,
		};
		_vesselParts.Add(vesselPart);
		_vesselPartNodes.Add(vesselPart, _currentPlacingPartNode);
		
		_currentPlacingPartNode = null;
		_currentPlacingPart = null;
		_state = State.Idle;
	}
}
