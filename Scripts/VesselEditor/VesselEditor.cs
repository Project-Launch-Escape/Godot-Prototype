using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class VesselEditor : Node3D
{
	[Export]
	public Camera3D Camera;
	public List<PackedScene> Parts = [];
	
	private const string PartSceneDirectory = "res://Resources/Parts/Scenes/";
	private const string VesselFileDirectory = "res://Vessels/";

	public static EditorState State = EditorState.Idle;
	private readonly Vector3 _origin = Vector3.Zero;

	private VesselPart _currentPlacingPart;
	private SnapPoint _attachingSnapPoint;
	private VesselPart _otherAttatchingPart;
	private SnapPoint _otherAttachingSnapPoint;

	private Node3D CopiedNode;

	private bool IsSnapped => _otherAttachingSnapPoint != null;
	private Basis _unsnappedRotation = Basis.Identity;
	
	private readonly List<VesselPart> _allParts = [];

	private float _placingDist = 5f;
	private const float SnapAngleTolerance = Mathf.Pi / 4;

	public static bool Shift;
	
	public enum EditorState
	{
		Idle,
		PlacingPart
	}


	public override void _Ready()
	{
		var fileNames = DirAccess.GetFilesAt(PartSceneDirectory);
		foreach (var fileName in fileNames)
		{
			var filePath = PartSceneDirectory + fileName;
			var partFile = ResourceLoader.Load<PackedScene>(filePath);
			Parts.Add(partFile);
		}
	}
	
	public override void _Process(double delta)
	{
		if (State is EditorState.PlacingPart) ProcessPlacingPartState();
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
			case InputEventKey { Keycode: Key.Shift} key:
				Shift = key.Pressed;
				break;
			case InputEventKey { Keycode: Key.A, Pressed: false} when State is EditorState.PlacingPart:
				_currentPlacingPart.RotateZ(Mathf.Pi / 2);
				_unsnappedRotation = _unsnappedRotation.Rotated(Vector3.Back, Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.D, Pressed: false} when State is EditorState.PlacingPart:
				_currentPlacingPart.RotateZ(-Mathf.Pi / 2);
				_unsnappedRotation = _unsnappedRotation.Rotated(Vector3.Back, -Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.W, Pressed: false} when State is EditorState.PlacingPart:
				_currentPlacingPart.RotateX(Mathf.Pi / 2);
				_unsnappedRotation = _unsnappedRotation.Rotated(Vector3.Right, Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.S, Pressed: false} when State is EditorState.PlacingPart:
				_currentPlacingPart.RotateX(-Mathf.Pi / 2);
				_unsnappedRotation = _unsnappedRotation.Rotated(Vector3.Right, -Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.Q, Pressed: false} when State is EditorState.PlacingPart:
				_currentPlacingPart.RotateY(Mathf.Pi / 2);
				_unsnappedRotation = _unsnappedRotation.Rotated(Vector3.Up, Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.E, Pressed: false} when State is EditorState.PlacingPart:
				_currentPlacingPart.RotateY(-Mathf.Pi / 2);
				_unsnappedRotation = _unsnappedRotation.Rotated(Vector3.Up, -Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.P, Pressed: false} when State is EditorState.PlacingPart:
				SaveVessel(_currentPlacingPart);
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } when State is EditorState.PlacingPart:
				PlacePart();
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } when State is EditorState.Idle:
				HandleMouseClick();
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp } when State is EditorState.PlacingPart && Shift:
				_placingDist += 1;
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelDown } when State is EditorState.PlacingPart && Shift:
				_placingDist -= 1;
				if (_placingDist < 1) _placingDist = 1;
				break;
		}
	}

	private void HandleMouseClick()
	{
		var cameraPos = Camera.GlobalPosition;
		var rayDirection = Camera.ProjectRayNormal(GetViewport().GetMousePosition());

		var physicsSpaceState = GetWorld3D().DirectSpaceState;
		var rayParameters = new PhysicsRayQueryParameters3D
		{
			From = cameraPos,
			To = cameraPos + rayDirection * 100f,
			CollisionMask = ColMask.Parts,
		};
		var rayResults = physicsSpaceState.IntersectRay(rayParameters);

		if (rayResults.Count == 0 || GizmoTranslate.GizmoActive) return;
		var selectedPart = (VesselPart)rayResults["collider"];

		_currentPlacingPart = selectedPart;
		State = EditorState.PlacingPart;

		if (_currentPlacingPart.GetParent() is VesselPart selectingPartParent)
		{
			foreach (var snapPoint in _currentPlacingPart.SnapPoints)
			{
				if (selectingPartParent != snapPoint.AttachedPart) continue;
				snapPoint.AttachedSnapPoint.Unattatch();
				snapPoint.Unattatch();
			}
		}
		selectedPart.ParentPart?.ChildParts.Remove(selectedPart);
		selectedPart.Reparent(this);
		_unsnappedRotation = _currentPlacingPart.Basis;
	}

	private void ProcessPlacingPartState()
	{
		_otherAttachingSnapPoint = null;
		_attachingSnapPoint = null;
		_otherAttatchingPart = null;
		
		var partTransform = GetPlacingPartTransform();
		_currentPlacingPart.Transform = partTransform;
		//if (!_isSnapped) _unsnappedRotation = _currentPlacingPart.Basis;
	}

	private Transform3D GetPlacingPartTransform()
	{
		var baseTransform = GetPartBaseTransform();

		Godot.Collections.Array<Rid> snapPointRids = [];
		foreach (var snapPoint in _currentPlacingPart.SnapPoints)
		{
			var rid = snapPoint.Collider.GetRid();
			snapPointRids.Add(rid);
		}

		var rayParameters = new PhysicsRayQueryParameters3D
		{
			From = Camera.GlobalPosition, CollisionMask = ColMask.SnapPoints, CollideWithAreas = true,
			CollideWithBodies = false, Exclude = snapPointRids
		};
		var physicsSpaceState = GetWorld3D().DirectSpaceState;

		Godot.Collections.Dictionary rayResult;
		SnapPoint placingSnapPoint = null;
		foreach (var snapPoint in _currentPlacingPart.SnapPoints)
		{
			if (snapPoint.Occupied) continue;
			placingSnapPoint = snapPoint;
			rayParameters.To = placingSnapPoint.GlobalPosition - _currentPlacingPart.GlobalPosition + baseTransform.Origin;

			rayResult = physicsSpaceState.IntersectRay(rayParameters);
			if (rayResult.Count == 0) continue;

			var otherSnapPoint = (SnapPoint)((Node3D)rayResult["collider"]).GetParentNode3D();
			var angleBetween = otherSnapPoint.Orientation.AngleTo(snapPoint.Orientation);
			if (angleBetween < Mathf.Pi - SnapAngleTolerance) continue;

			_otherAttachingSnapPoint = otherSnapPoint;
			_otherAttatchingPart = otherSnapPoint.Parent;
			break;
		}

		if (_otherAttachingSnapPoint == null) return baseTransform;

		_attachingSnapPoint = placingSnapPoint;
		if (_otherAttachingSnapPoint.Occupied) return baseTransform;

		var newOrientation = _otherAttachingSnapPoint.Orientation * placingSnapPoint!.LocalOrientation;
		var newBasis = new Basis(newOrientation);
		newBasis = newBasis.Rotated(newBasis.Z.Normalized(), Mathf.Pi);

		var newPosition = _otherAttachingSnapPoint.GlobalPosition - placingSnapPoint!.GlobalPosition + _currentPlacingPart.GlobalPosition;
		
		var newTransform = new Transform3D(newBasis, newPosition);
		return newTransform;
	}
	
	private Transform3D GetPartBaseTransform()
	{
		var cameraPos = Camera.GlobalPosition;
		var rayDirection = Camera.ProjectRayNormal(GetViewport().GetMousePosition());

		var physicsSpaceState = GetWorld3D().DirectSpaceState;
		var rayParameters = new PhysicsRayQueryParameters3D
		{
			From = cameraPos,
			To = cameraPos + rayDirection * _placingDist * 1.25f,
			CollisionMask = ColMask.Parts,
			Exclude = new Godot.Collections.Array<Rid>(new[] { _currentPlacingPart.GetRid() })
		};
		var rayResult = physicsSpaceState.IntersectRay(rayParameters);
		
		var newTransform = Transform3D.Identity;
		Vector3 newPos;
		
		if (rayResult.Count > 0 && _currentPlacingPart.SurfaceAttatchable)
		{
			var intersectPosition = (Vector3)rayResult["position"];
			var intersectNormal = (Vector3)rayResult["normal"];
			var intersectPart = (VesselPart)rayResult["collider"];

			_otherAttatchingPart = intersectPart;
			
			var surfaceAttatchPoint = _currentPlacingPart.SurfaceAttatchPoint;
			newTransform.Basis = Basis.LookingAt(-intersectNormal) * surfaceAttatchPoint.Basis * _unsnappedRotation;
			
			newPos = intersectPosition - surfaceAttatchPoint.GlobalPosition + _currentPlacingPart.GlobalPosition;
			newTransform.Origin = newPos;
			return newTransform;
		}
		
		var cameraPositionWithoutY = Camera.GlobalPosition with { Y = 0 };
		var planeNormal = (_origin - cameraPositionWithoutY).Normalized();
		var plane = new Plane(planeNormal, _placingDist - cameraPositionWithoutY.Length());
		
		var rayIntersection = plane.IntersectsRay(cameraPos, rayDirection);
		newPos = rayIntersection ?? _currentPlacingPart.Position;
		newTransform.Origin = newPos;
		newTransform.Basis = _unsnappedRotation;
		return newTransform;
	}
	
	// All of this is temporary for now
	private void SelectPart(int n)
	{
		var distTemp = _placingDist;
		DeselectPart();
		if (n < 0 || n >= Parts.Count) return;
		_placingDist = distTemp; // So it doesn't reset
		
		State = EditorState.PlacingPart;
		_currentPlacingPart = (VesselPart)Parts[n].Instantiate();
		_unsnappedRotation = Basis.Identity;
		
		AddChild(_currentPlacingPart);
	}

	private void DeselectPart()
	{
		_currentPlacingPart?.QueueFree();
		_currentPlacingPart = null;
		State = EditorState.Idle;
		_placingDist = 5f;
	}

	private void PlacePart()
	{
		_allParts.Add(_currentPlacingPart);

		if (_otherAttatchingPart != null)
		{
			_currentPlacingPart.Reparent(_otherAttatchingPart);
			_otherAttatchingPart.ChildParts.Add(_currentPlacingPart);
		}
		if (_otherAttachingSnapPoint != null)
		{
			_otherAttachingSnapPoint.Occupied = true;
			if (_attachingSnapPoint != null)
			{
				_otherAttachingSnapPoint.AttachTo(_attachingSnapPoint);
				_attachingSnapPoint.AttachTo(_otherAttachingSnapPoint);
			}
		}
		
		_currentPlacingPart = null;
		State = EditorState.Idle;
	}

	private void SaveVessel(VesselPart vesselRootPart)
	{
		foreach (var descendant in vesselRootPart.GetAllDescendantParts())
		{
			descendant.Owner = vesselRootPart;
		}
		
		var scene = new PackedScene();
		scene.Pack(vesselRootPart);
		var error = ResourceSaver.Save(scene, VesselFileDirectory + "save.tscn");
		if (error is not Error.Ok) GD.PrintErr(error);
	}
}
