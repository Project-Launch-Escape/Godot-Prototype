using Godot;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class GizmoTranslate : Node3D
{
	private const float RayLength = 1000.0f;
	public static GizmoType SelectedGizmo = GizmoType.None;
	public static Node3D SelectedObject;
	public static bool GizmoActive => SelectedObject != null;
	private Vector3 _startRotation;

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
		switch (inputEvent)
		{
			case InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: false } when VesselEditor.State is VesselEditor.EditorState.Idle :
				SelectHoveredPart();
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } when VesselEditor.State is VesselEditor.EditorState.Idle :
				SelectHoveredGizmo();
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } :
			{
				SelectedGizmo = GizmoType.None;
				Rotation = Vector3.Zero;
				if (SelectedObject != null) _startRotation = SelectedObject.Rotation;
				break;
			}
			case InputEventMouse:
				HandleMouseMovement();
				break;
		}
	}

	private void SelectHoveredPart()
	{
		var rayParameters = GetMouseDirectionRayParameters();
		rayParameters.CollisionMask = ColMask.Parts;

		var rayResult = GetWorld3D().DirectSpaceState.IntersectRay(rayParameters);
		if (rayResult.Count == 0)
		{
			Visible = false;
			SelectedObject = null;
			return;
		}
		
		Visible = true;
		SelectedObject = (Node3D)rayResult["collider"];
		Position = SelectedObject.GlobalPosition;
		Rotation = new Vector3(0, 0, 0);
		_startRotation = SelectedObject.Rotation;
	}

	private void SelectHoveredGizmo()
	{
		var rayParameters = GetMouseDirectionRayParameters();
		rayParameters.CollisionMask = ColMask.Gizmos;

		var rayResult = GetWorld3D().DirectSpaceState.IntersectRay(rayParameters);
		if (rayResult.Count == 0)
		{
			SelectedGizmo = GizmoType.None;
			return;
		}
		
		var res = (Node)rayResult["collider"];
		SelectedGizmo = (string)res.Name switch
		{
			// "T" is translation gizmos, "R" is rotation gizmos
			"RED T Handle" => GizmoType.TransRed,
			"GREEN T Handle" => GizmoType.TransGreen,
			"BLUE T Handle" => GizmoType.TransBlue,
			"RED R Handle" => GizmoType.RotRed,
			"GREEN R Handle" => GizmoType.RotGreen,
			"BLUE R Handle" => GizmoType.RotBlue,
			_ => SelectedGizmo
		};
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

	private void HandleMouseMovement()
	{
		if (SelectedGizmo is GizmoType.None) return;
		
		var gizmos = this;
		var mousePosition = GetViewport().GetMousePosition();
		var camera = GetViewport().GetCamera3D();

		var axis = AxisFromGizmo(SelectedGizmo);

		var gizmosPos = gizmos.GlobalPosition;			
		var cameraPos = camera.GlobalPosition; // This might need to change according to reference frame or something like that idk	
		var normal = (cameraPos - gizmosPos).Normalized(); // Normal Vector of the plane
			
		var rayStart = camera.ProjectRayOrigin(mousePosition);
		var rayEnd = rayStart + camera.ProjectRayNormal(mousePosition) * RayLength;
			
		var virtualPlane = new Plane(normal, gizmosPos);
		var rayIntersection = virtualPlane.IntersectsRay(rayStart, rayEnd);

		if (rayIntersection == null) return;
		
		var posDelta = (Vector3)rayIntersection - gizmos.Position; // Change Of Position
		
		if (IsTranslational(SelectedGizmo))
		{
			posDelta *= axis; // Only Change Position On Axis
			gizmos.Position += posDelta; // Apply Change of Position
			SelectedObject.GlobalPosition = gizmos.Position;
		}
		else
		{
			posDelta = ((Vector3.One - axis) * posDelta).Normalized(); // Only Change Position On Anti-Axis (the 2 others Axis)	
					
			float theta = SelectedGizmo switch
			{
				GizmoType.RotRed => -Mathf.Atan2(posDelta.Y, posDelta.Z),
				GizmoType.RotGreen => -Mathf.Atan2(posDelta.Z, posDelta.X),
				GizmoType.RotBlue => -Mathf.Atan2(posDelta.X, posDelta.Y),
				_ => 0
			};
			gizmos.Rotation = axis * theta; // Apply Change of Rotation
			SelectedObject.Rotation = _startRotation;
			SelectedObject.Rotate(axis, theta); // startRotation + A * θ;
		}
	}
}
