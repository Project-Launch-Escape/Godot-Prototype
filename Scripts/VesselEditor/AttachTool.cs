using Godot;
using GodotPrototype.Scripts.VesselEditor.EditorUI;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor;

[Icon("res://Resources/UITextures/VesselEditor/AttachIcon.png")]
public partial class AttachTool : Node3D, IToolable
{
	public static AttachTool ToolNode;
	
	private SnapPoint _childSnapPoint;
	private SnapPoint _hoveredSnapPoint;


	public override void _Ready()
	{
		if (ToolNode != null) GD.PrintErr("Multiple AttachTool Nodes!");
		ToolNode = this;
	}

	public bool IsToolActive { get; set; }

	public void HandleTool(InputEventMouseButton mouseInput, bool shiftModifier, bool altModifier)
	{
		if (mouseInput.Pressed) return;
		
		if (_childSnapPoint == null)
		{
			_childSnapPoint = _hoveredSnapPoint;
			_childSnapPoint?.SetVisualState(SnapPoint.SnapPointVisualState.Selected);
			return;
		}

		var parentSnapPoint = _hoveredSnapPoint;
		if (parentSnapPoint == null) return;

		_childSnapPoint.SnapParentPartToSnapPoint(parentSnapPoint);
		
		parentSnapPoint.SetVisualState(SnapPoint.SnapPointVisualState.Normal);
		_childSnapPoint.SetVisualState(SnapPoint.SnapPointVisualState.Normal);
		
		_childSnapPoint = null;
	}

	public override void _Process(double delta)
	{
		if (!Editor.IsToolEnabled(EditorTool.Attach)) return;
		
		var newHoveredSnapPoint = GetMouseHoveredSnapPoint();
		if (newHoveredSnapPoint != _hoveredSnapPoint)
		{
			if (newHoveredSnapPoint != _childSnapPoint) newHoveredSnapPoint?.SetVisualState(SnapPoint.SnapPointVisualState.Hovered);
			if (_hoveredSnapPoint != _childSnapPoint) _hoveredSnapPoint?.SetVisualState(SnapPoint.SnapPointVisualState.Normal);
			_hoveredSnapPoint = newHoveredSnapPoint;
		}
	}

	public void OnToolEnable()
	{
		SnapPoint.SetGlobalVisibility(true);
	}
	public void OnToolDisable()
	{
		SnapPoint.SetGlobalVisibility(false);
	}


	public static SnapPoint GetMouseHoveredSnapPoint()
	{
		const float rayLength = 500f;
		var camera = Editor.Camera;
		var cameraPos = camera.GlobalPosition;
		var rayDirection = camera.ProjectRayNormal(Editor.EditorNode.GetViewport().GetMousePosition());

		var physicsSpaceState = Editor.EditorNode.GetWorld3D().DirectSpaceState;
		
		var rayParameters = new PhysicsRayQueryParameters3D
		{
			From = cameraPos,
			To = cameraPos + rayDirection * rayLength,
			CollideWithAreas = true,
			CollideWithBodies = false,
			CollisionMask = ColMask.SnapPoints
		};
		var rayResults = physicsSpaceState.IntersectRay(rayParameters);
		return rayResults.Count > 0 ? (SnapPoint)((Node3D)rayResults["collider"]).GetParentNode3D() : null;
	}
}
