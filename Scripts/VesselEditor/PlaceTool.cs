using System.Text.Json;
using Godot;
using Godot.Collections;
using GodotPrototype.Scripts.VesselEditor.EditorUI;
using GodotPrototype.Scripts.VesselEditor.FileInterfacing;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class PlaceTool : Node3D, IToolable
{
	public static PlaceTool ToolNode;
	private VesselEditorCamera Camera => Editor.Camera;
	
	private VesselPart _currentPlacingPart;
	private SnapPoint _attachingSnapPoint;
	private VesselPart _otherAttachingPart;
	private SnapPoint _otherAttachingSnapPoint;

	private bool IsAttaching => _otherAttachingPart != null;
	private bool IsSnapping => _otherAttachingSnapPoint != null;
	private bool IsSurfaceAttaching => _otherAttachingSnapPoint == null && _otherAttachingPart != null;
	
	private float _placingDist = 5f;

	public static bool Shift;

	public bool IsToolActive { get; set; } // PlaceTool is active when a part is being placed with the cursor
	
	public override void _Ready()
	{
		if (ToolNode != null) GD.PrintErr("Multiple PlaceTool Nodes!");
		ToolNode = this;
	}
	
	public override void _Process(double delta)
	{
		if (IsToolActive) ProcessPlacingPartState();
	}
	

	public void HandleTool(InputEventMouseButton mouseInput, bool shiftModifier, bool altModifier)
	{ // Only handles picking up placed parts
		
		if (mouseInput.Pressed) return;
		if (IsToolActive)
		{
			PlacePart();
			return;
		}
		
		var rayResults = Editor.GetMouseHoveredRayResults();
		if (rayResults.Count == 0 || TranslateTool.GizmoActive) return;
		
		var selectedPart = (VesselPart)rayResults["collider"];
		if (Shift)
		{
			selectedPart = Editor.DuplicatePart(selectedPart);
			AddChild(selectedPart);
		}

		_currentPlacingPart = selectedPart;
		IsToolActive = true;
		SnapPoint.SetVisualVisibility(true);

		if (_currentPlacingPart.GetParent() is VesselPart selectingPartParent)
		{
			foreach (var snapPoint in _currentPlacingPart.SnapPoints)
			{
				if (selectingPartParent != snapPoint.AttachedPart) continue;
				snapPoint.AttachedSnapPoint.Unattatch();
				snapPoint.Unattatch();
			}
		}
		selectedPart.Reparent(this);
	}
	
	private void ProcessPlacingPartState()
	{
		_otherAttachingSnapPoint = null;
		_attachingSnapPoint = null;
		_otherAttachingPart = null;
		
		var partTransform = GetPlacingPartTransform();
		_currentPlacingPart.Transform = partTransform;
	}

	private Transform3D GetPlacingPartTransform()
	{
		var baseTransform = GetPartBaseTransform();
		if (Input.IsKeyPressed(Key.Alt)) return baseTransform;
		
		var ingoredParts = new List<VesselPart>(_currentPlacingPart.GetAllDescendantParts()) { _currentPlacingPart };
		Array<Rid> snapPointRids = [];
		foreach (var part in ingoredParts)
		{
			foreach (var snapPoint in part.SnapPoints)
			{
				snapPointRids.Add(snapPoint.Collider.GetRid());
			}
		}

		var rayParameters = new PhysicsRayQueryParameters3D
		{
			From = Camera.GlobalPosition, 
			CollisionMask = ColMask.SnapPoints, 
			CollideWithAreas = true,
			CollideWithBodies = false, 
			Exclude = snapPointRids
		};
		
		var physicsSpaceState = GetWorld3D().DirectSpaceState;
		SnapPoint placingSnapPoint = null;
		foreach (var snapPoint in _currentPlacingPart.SnapPoints)
		{
			if (snapPoint.Occupied) continue;
			
			placingSnapPoint = snapPoint;
			rayParameters.To = baseTransform.Origin + (placingSnapPoint.GlobalPosition - placingSnapPoint.ParentPart.GlobalPosition);

			var rayResult = physicsSpaceState.IntersectRay(rayParameters);
			if (rayResult.Count == 0) continue;
			
			var otherSnapPoint = (SnapPoint)((Node3D)rayResult["collider"]).GetParentNode3D();
			if (otherSnapPoint.Occupied) continue;
			const float snapAngleTolerance = Mathf.Pi / 4;
			
			var placingDirection = snapPoint.GlobalBasis.Y;
			var otherDirection = otherSnapPoint.GlobalBasis.Y;
			
			var angleBetween = placingDirection.AngleTo(-otherDirection);
			if (angleBetween > snapAngleTolerance) continue;

			_otherAttachingSnapPoint = otherSnapPoint;
			_otherAttachingPart = otherSnapPoint.ParentPart;
			break;
		}

		if (_otherAttachingSnapPoint == null) return baseTransform;

		_attachingSnapPoint = placingSnapPoint;

		var newBasis = _otherAttachingSnapPoint.GlobalBasis * placingSnapPoint!.Basis;
		newBasis = newBasis.Rotated(newBasis.Z.Normalized(), Mathf.Pi);

		var newPosition = _otherAttachingSnapPoint.GlobalPosition - placingSnapPoint!.GlobalPosition + _currentPlacingPart.GlobalPosition;
		
		var newTransform = new Transform3D(newBasis, newPosition);
		return newTransform;
	}
	
	private Transform3D GetPartBaseTransform()
	{
		Array<Rid> descendentRids = [_currentPlacingPart.GetRid()];
		foreach (var descendant in _currentPlacingPart.GetAllDescendantParts()) { descendentRids.Add(descendant.GetRid()); }
		var rayResult = Editor.GetMouseHoveredRayResults(_placingDist * 1.25f, descendentRids );
		
		var newTransform = Transform3D.Identity;
		Vector3 newPos;
		
		if (rayResult.Count > 0 && _currentPlacingPart.SurfaceAttatchable)
		{
			var intersectPosition = (Vector3)rayResult["position"];
			var intersectNormal = (Vector3)rayResult["normal"];
			var intersectPart = (VesselPart)rayResult["collider"];

			_otherAttachingPart = intersectPart;
			
			var surfaceAttatchPoint = _currentPlacingPart.SurfaceAttatchPoint;

			var result = Basis.Identity;
			if (intersectNormal != Vector3.Up)
			{
				var basisZ = intersectNormal.Normalized();
				var basisX = _otherAttachingPart.Basis.Y.Cross(basisZ).Normalized();
				var basisY = basisZ.Cross(basisX).Normalized();
				result = new Basis(basisX, basisY, basisZ);
			}
			newTransform.Basis = result * surfaceAttatchPoint.Basis * _currentPlacingPart.UnsnappedBasis;
			
			newPos = intersectPosition - surfaceAttatchPoint.GlobalPosition + _currentPlacingPart.GlobalPosition;
			newTransform.Origin = newPos;
			return newTransform;
		}
		var mouseDirection = Camera.ProjectRayNormal(GetViewport().GetMousePosition());
		
		var cameraPositionWithoutY = Camera.GlobalPosition - Camera.Origin with { Y = 0 };
		var planeNormal = -cameraPositionWithoutY.Normalized();
		var plane = new Plane(planeNormal, planeNormal * _placingDist + cameraPositionWithoutY);
		
		var rayIntersection = plane.IntersectsRay(Camera.GlobalPosition - Camera.Origin, mouseDirection);
		newPos = rayIntersection ?? _currentPlacingPart.Position;
		
		newTransform.Origin = newPos + Camera.Origin;
		newTransform.Basis = _currentPlacingPart.UnsnappedBasis;
		
		return newTransform;
	}
	
	
	public void SelectPart(int n)
	{
		var partDefs = PartDefinition.PartDefinitions;
		
		var newPart = (VesselPart)partDefs[n].Scene.Instantiate();
		newPart.PartDefID = partDefs[n].ID;
		
		SelectPart(newPart);
	}

	public void SelectPart(VesselPart part)
	{
		var distTemp = _placingDist;
		DeselectPart();
		_placingDist = distTemp; // So it doesn't reset
		IsToolActive = true;
		SnapPoint.SetVisualVisibility(true);

		_currentPlacingPart = part;
		if (!part.IsInsideTree()) AddChild(_currentPlacingPart);
	}

	private void DeselectPart()
	{
		if (_currentPlacingPart != null)
		{
			_currentPlacingPart.QueueFree();
			_currentPlacingPart = null;
		}
		
		IsToolActive = false;
		if (!Editor.IsToolEnabled(EditorTool.Attach)) SnapPoint.SetVisualVisibility(false);
		_placingDist = 5f;
	}
	private void PlacePart()
	{
		if (IsAttaching)
		{
			if (!IsSurfaceAttaching) // Runs only when snapping , not surface attaching
			{
				SnapPoint.AttachSnapPointTo(_attachingSnapPoint, _otherAttachingSnapPoint);
			}
			else
			{
				if (_currentPlacingPart.GetAllDescendantParts().Contains(_otherAttachingPart)) return;
				_currentPlacingPart.Reparent(_otherAttachingPart);
			}
		}
		
		_currentPlacingPart = null;
		IsToolActive = false;
		if (!Editor.IsToolEnabled(EditorTool.Attach)) SnapPoint.SetVisualVisibility(false);
	}

	private void CopyPlacingToClipboard()
	{
		var json = VesselFileTools.GetPartTreeJsonString(_currentPlacingPart);
		DisplayServer.ClipboardSet(json);
	}
	private void PasteVesselFromClipboard()
	{
		var json = DisplayServer.ClipboardGet();
		
		try
		{
			var rootPart = VesselFileTools.GetVesselRootFromJson(json);
			SelectPart(rootPart);
		}
		catch (JsonException)
		{
			GD.PrintErr("Pasted string failed to parse");
		}
	}

	public void OnToolEnable()
	{
		
	}

	public void OnToolDisable()
	{
		
	}
	
	public override void _UnhandledInput(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventKey { Keycode: Key.Escape, Pressed: false }:
				DeselectPart();
				break;
			case InputEventKey { Keycode: Key.Shift} key:
				Shift = key.Pressed;
				break;
			case InputEventKey { Keycode: Key.C,Pressed: false, CtrlPressed: true, ShiftPressed: false}:
				CopyPlacingToClipboard();
				break;
			case InputEventKey { Keycode: Key.V, Pressed: false, CtrlPressed: true, ShiftPressed: false}:
				PasteVesselFromClipboard();
				break;
			case InputEventKey { Keycode: Key.A, Pressed: false} when IsToolActive:
				_currentPlacingPart.RotateZ(Mathf.Pi / 2);
				_currentPlacingPart.UnsnappedBasis = _currentPlacingPart.UnsnappedBasis.Rotated(Vector3.Back, Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.D, Pressed: false} when IsToolActive:
				_currentPlacingPart.RotateZ(-Mathf.Pi / 2);
				_currentPlacingPart.UnsnappedBasis = _currentPlacingPart.UnsnappedBasis.Rotated(Vector3.Back, -Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.W, Pressed: false} when IsToolActive:
				_currentPlacingPart.RotateX(Mathf.Pi / 2);
				_currentPlacingPart.UnsnappedBasis = _currentPlacingPart.UnsnappedBasis.Rotated(Vector3.Right, Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.S, Pressed: false} when IsToolActive:
				_currentPlacingPart.RotateX(-Mathf.Pi / 2);
				_currentPlacingPart.UnsnappedBasis = _currentPlacingPart.UnsnappedBasis.Rotated(Vector3.Right, -Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.Q, Pressed: false} when IsToolActive:
				_currentPlacingPart.RotateY(Mathf.Pi / 2);
				_currentPlacingPart.UnsnappedBasis = _currentPlacingPart.UnsnappedBasis.Rotated(Vector3.Up, Mathf.Pi / 2);
				break;
			case InputEventKey { Keycode: Key.E, Pressed: false} when IsToolActive:
				_currentPlacingPart.RotateY(-Mathf.Pi / 2);
				_currentPlacingPart.UnsnappedBasis = _currentPlacingPart.UnsnappedBasis.Rotated(Vector3.Up, -Mathf.Pi / 2);
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp } when IsToolActive && Shift:
				_placingDist += 1;
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelDown } when IsToolActive && Shift:
				_placingDist -= 1;
				if (_placingDist < 1) _placingDist = 1;
				break;
		}
	}
}
