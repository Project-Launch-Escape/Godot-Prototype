using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

[Icon("res://Resources/UITextures/VesselEditor/ModifyIcon.png")]
public partial class ContextMenuController : Node, IToolable
{
	[Export] public Godot.Collections.Dictionary<ComponentType, PackedScene> ComponentMenus;
	[Export] public PackedScene ContextMenuScene;
	[Export] public PackedScene ValueInputScene;
	[Export] public PackedScene ContextButtonScene;
	
	public static ContextMenuController ControllerNode;
	
	public bool IsToolActive { get; set; }
	
	public override void _Ready()
	{
		if (ControllerNode != null) GD.PrintErr("Multiple Context Menu Controller Nodes!");
		ControllerNode = this;
	}

	private void CreateContextMenu(VesselPart part)
	{
		var camera = GlobalValues.CurrentScene switch
		{
			SceneType.FlightScene => GlobalValues.LocalSpaceCamera,
			SceneType.VesselEditor => Editor.Camera,
			_ => throw new NotSupportedException("Context Menus are not supported in this scene!")
		};
		var screenPos = camera.UnprojectPosition(part.GlobalPosition);

		var newContextMenu = ContextMenuScene.Instantiate<ContextMenu>();
		newContextMenu.ParentPart = part;
		AddChild(newContextMenu);
		newContextMenu.Position = screenPos;
	}
	
	public void HandleTool(InputEventMouseButton mouseInput, bool shiftModifier, bool altModifier)
	{
		if (mouseInput.Pressed) return;
		
		var hoveredPart = PartHover.GetMouseHoveredPart(); // TODO: Add logic for FlightScene compatibility
		if (hoveredPart == null) return;
		CreateContextMenu(hoveredPart);
	}

	public void OnToolEnable()
	{
		
	}

	public void OnToolDisable()
	{
		
	}
	public override void _UnhandledInput(InputEvent inputEvent) // Overrided behavior for use in FlightScene
	{
		if (GlobalValues.CurrentScene is not SceneType.FlightScene) return;
		if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Right } mouseButton) HandleTool(mouseButton, false, false);
	}
}
