using Godot;
using GodotPrototype.Scripts.VesselEditor.EditorUI;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor;

[Icon("res://Resources/UITextures/VesselEditor/TransformIcon.png")]
public partial class TranslateTool : Node3D, IToolable
{
	public static TranslateTool ToolNode;
	
	private const float RayLength = 1000.0f;
	private GizmoType _selectedGizmo = GizmoType.None;
	private VesselPart _selectedPart;
	public static bool GizmoActive => ToolNode._selectedPart != null;
	
	private Transform3D _startTransform;
	private Basis _startUnsnappedBasis;
	private Vector3 _startMousePos;

	private Vector3 _truePosition;

	[Export] private float _coarseGridSize; // When holding Shift
	[Export] private float _fineGridSize; // When holding Alt
	
	// In degrees
	[Export] private float _coarseAngleSnapInterval; // When holding Shift
	[Export] private float _fineAngleSnapInterval; // When holding Alt

	public bool IsToolActive { get; set; }
	
	public enum GizmoType
	{
		None = -1,
		
		TransRed = 0,
		TransGreen = 1,
		TransBlue = 2,
		
		RotRed = 3,
		RotGreen = 4,
		RotBlue = 5
	}

	public override void _Ready()
	{
		if (ToolNode != null) GD.PrintErr("Multiple TranslateTool Nodes!");
		ToolNode = this;
	}

	private static Vector3 AxisFromGizmo(GizmoType gizmo)
	{
		return gizmo switch
		{
			GizmoType.TransRed => Vector3.Right, // x axis
			GizmoType.TransGreen => Vector3.Up, // y axis
			GizmoType.TransBlue => Vector3.Back, // z axis
			
			GizmoType.RotRed => Vector3.Right, // x axis
			GizmoType.RotGreen => Vector3.Up, // y axis
			GizmoType.RotBlue => Vector3.Back, // z axis
			_ => new Vector3(0, 0, 0)
		};
	}

	private static bool IsTranslational(GizmoType gizmoType) =>
		gizmoType is GizmoType.TransBlue or GizmoType.TransRed or GizmoType.TransGreen;

	private static bool IsRotational(GizmoType gizmoType) =>
		gizmoType is GizmoType.RotBlue or GizmoType.RotRed or GizmoType.RotGreen; 
	
	public override void _Input(InputEvent inputEvent)
	{
		if (!Editor.IsToolEnabled(EditorTool.Transform)) return;
		switch (inputEvent)
		{
			case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } when !Editor.IsToolActive(EditorTool.Place) :
				SelectHoveredGizmo();
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } :
			{
				_selectedGizmo = GizmoType.None;
				Rotation = Vector3.Zero;
				if (_selectedPart != null)
				{
					_startTransform = _selectedPart.Transform;
					_startUnsnappedBasis = _selectedPart.UnsnappedBasis;
				}
				break;
			}
			case InputEventMouse:
				HandleMouseMovement();
				break;
		}
	}

	public void DeactivateGizmo()
	{
		Visible = false;
		_selectedPart = null;
	}

	

	public void HandleTool(InputEventMouseButton mouseInput, bool shiftModifier, bool altModifier)
	{
		if (mouseInput.Pressed) return;
		if (shiftModifier)
		{
			var selectedPart = Editor.GetMouseHoveredPart();
			selectedPart.UnsnappedBasis = Basis.Identity;
			selectedPart.Basis = Basis.Identity;
			return;
		}
		
		SelectHoveredPart();
	}
	private void SelectHoveredPart()
	{
		var rayParameters = GetMouseDirectionRayParameters();
		rayParameters.CollisionMask = ColMask.Parts;

		var rayResult = GetWorld3D().DirectSpaceState.IntersectRay(rayParameters);
		if (rayResult.Count == 0)
		{
			DeactivateGizmo();
			return;
		}
		
		Visible = true;
		_selectedPart = (VesselPart)rayResult["collider"];
		Position = _selectedPart.GlobalPosition;
		Rotation = new Vector3(0, 0, 0);
		_startTransform = _selectedPart.Transform;
		_startUnsnappedBasis = _selectedPart.UnsnappedBasis;
	}

	private void SelectHoveredGizmo()
	{
		var rayParameters = GetMouseDirectionRayParameters();
		rayParameters.CollisionMask = ColMask.Gizmos;

		var rayResult = GetWorld3D().DirectSpaceState.IntersectRay(rayParameters);
		if (rayResult.Count == 0)
		{
			_selectedGizmo = GizmoType.None;
			return;
		}
		
		var res = (Node)rayResult["collider"];
		_selectedGizmo = (string)res.Name switch
		{
			// "T" is translation gizmos, "R" is rotation gizmos
			"RED T Handle" => GizmoType.TransRed,
			"GREEN T Handle" => GizmoType.TransGreen,
			"BLUE T Handle" => GizmoType.TransBlue,
			"RED R Handle" => GizmoType.RotRed,
			"GREEN R Handle" => GizmoType.RotGreen,
			"BLUE R Handle" => GizmoType.RotBlue,
			_ => _selectedGizmo
		};
		_startMousePos = GetGizmoMousePos();
	}

	private PhysicsRayQueryParameters3D GetMouseDirectionRayParameters()
	{
		var mousePosition = GetViewport().GetMousePosition();
		var camera3D = GetViewport().GetCamera3D();
		var rayStart = camera3D.Position;
		var rayEnd = rayStart + camera3D.ProjectRayNormal(mousePosition) * RayLength;

		var rayParameters = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd);
		rayParameters.CollideWithAreas = false;
		rayParameters.CollisionMask = 0;
		return rayParameters;
	}

	private Vector3 GetGizmoMousePos()
	{
		var mousePosition = GetViewport().GetMousePosition();
		var camera = GetViewport().GetCamera3D();

		var gizmosPos = _startTransform.Origin; //GlobalPosition;
		var cameraPos = camera.GlobalPosition; // This might need to change according to reference frame or something like that idk	
		var normal = (cameraPos - gizmosPos).Normalized(); // Normal Vector of the plane
			
		var rayStart = camera.ProjectRayOrigin(mousePosition);
		var rayEnd = camera.ProjectRayNormal(mousePosition);
			
		var gizmoPlane = new Plane(normal, gizmosPos);
		var rayIntersection = gizmoPlane.IntersectsRay(rayStart, rayEnd);

		if (rayIntersection == null) return _startMousePos;
		
		return (Vector3)rayIntersection - gizmosPos; // Change of Position of mouse cursor
	}

	private void HandleMouseMovement()
	{
		if (_selectedGizmo is GizmoType.None) return;

		var axis = AxisFromGizmo(_selectedGizmo);
		var mousePos = GetGizmoMousePos();
		
		if (IsTranslational(_selectedGizmo))
		{
			HandleTranslation(axis, mousePos);
		}
		else
		{
			HandleRotation(axis, mousePos);
		}
	}

	private void HandleTranslation(Vector3 axis, Vector3 mousePos)
	{
		mousePos -= _startMousePos;
		mousePos *= axis; // Only Change Position On Axis

		var newPosition = _startTransform.Origin + mousePos;

		newPosition = SnapPosition(newPosition);
			
		_selectedPart.Position = newPosition;
		Position = _selectedPart.GlobalPosition; // Apply Change of Position
	}

	private void HandleRotation(Vector3 axis, Vector3 mousePos)
	{
		//TODO: Fix Rotation Snapping with Child Parts
		mousePos = ((Vector3.One - axis) * mousePos).Normalized(); // Only track change in position on plane perpendicular to the axis of rotation

		float theta = _selectedGizmo switch
		{
			GizmoType.RotRed => -Mathf.Atan2(mousePos.Y, mousePos.Z),
			GizmoType.RotGreen => -Mathf.Atan2(mousePos.Z, mousePos.X),
			GizmoType.RotBlue => -Mathf.Atan2(mousePos.X, mousePos.Y),
			_ => 0
		};
		theta = SnapAngle(theta);
			
		Rotation = axis * theta; // Apply Change of Rotation
		_selectedPart.Basis = _startTransform.Basis.Rotated(axis, theta); // startRotation + A * θ;

		_selectedPart.UnsnappedBasis = _startUnsnappedBasis.Rotated(axis, theta);
	}

	private Vector3 SnapPosition(Vector3 newPosition)
	{
		if (!Input.IsKeyPressed(Key.Shift) && !Input.IsKeyPressed(Key.Alt)) return newPosition; // Handle Snapping
		
		var gridSize = Input.IsKeyPressed(Key.Shift) ? _coarseGridSize : _fineGridSize;
				
		newPosition /= gridSize;
		newPosition = new Vector3(Mathf.Round(newPosition.X), Mathf.Round(newPosition.Y), Mathf.Round(newPosition.Z));
		newPosition *= gridSize;
			
		_startTransform = _selectedPart.Transform; // To prevent snaps from reverting once letting go of modifier
		return newPosition;
	}

	private float SnapAngle(float theta)
	{
		if (!Input.IsKeyPressed(Key.Shift) && !Input.IsKeyPressed(Key.Alt)) return theta;
		
		var snapInterval = Input.IsKeyPressed(Key.Shift) ? _coarseAngleSnapInterval : _fineAngleSnapInterval;
		theta = Mathf.RadToDeg(theta);
		theta = snapInterval * Mathf.Round(theta / snapInterval);
		theta = Mathf.DegToRad(theta);
		return theta;
	}
	
	public void OnToolEnable()
	{ 
		
	}

	public void OnToolDisable()
	{
		DeactivateGizmo();
	}
}
