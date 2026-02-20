using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

[Icon("res://Resources/UITextures/VesselEditor/ModifyIcon.png")]
public partial class ContextMenuController : Node, IToolable
{
	[Export] public Godot.Collections.Dictionary<ComponentType, PackedScene> ComponentMenus;
	[Export] public PackedScene ContextMenuScene;
	[Export] public PackedScene ValueInputScene;
		
	public static ContextMenuController ControllerNode;
	
	public bool IsToolActive { get; set; }
	
	public override void _Ready()
	{
		if (ControllerNode != null) GD.PrintErr("Multiple Context Menu Controller Nodes!");
		ControllerNode = this;
	}

	private void CreateContextMenu(VesselPart part)
	{
		var camera = GetViewport().GetCamera3D();
		var screenPos = camera.UnprojectPosition(part.GlobalPosition);

		var newContextMenu = ContextMenuScene.Instantiate<ContextMenu>();
		newContextMenu.ParentPart = part;
		AddChild(newContextMenu);
		newContextMenu.Position = screenPos;
	}
	
	public void HandleTool(InputEventMouseButton mouseInput, bool shiftModifier, bool altModifier)
	{
		if (mouseInput.Pressed) return;
		
		var hoveredPart = Editor.GetMouseHoveredPart();
		if (hoveredPart == null) return;
		CreateContextMenu(hoveredPart);
	}

	public void OnToolEnable()
	{
		
	}

	public void OnToolDisable()
	{
		
	}
}
