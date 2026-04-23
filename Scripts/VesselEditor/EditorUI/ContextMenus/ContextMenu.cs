using Godot;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class ContextMenu : Draggable
{
	[Export] private VBoxContainer _componentMenuContainer;
	[Export] private Label _partNameLabel;
	
	public VesselPart ParentPart;
	private List<Control> _componentMenus = [];

	public static Godot.Collections.Dictionary<ComponentType, PackedScene> ComponentMenuScenes => ContextMenuController.ControllerNode.ComponentMenus;

	public override void _Ready()
	{
		AddComponentMenu(null);
		foreach (var component in ParentPart.Components)
		{
			AddComponentMenu(component);
		}

		_partNameLabel.Text = ParentPart.PartName;
	}

	private void AddComponentMenu(PartComponent component)
	{
		var componentMenu = ComponentMenuScenes[component?.ComponentType ?? ComponentType.Default].Instantiate<MenuComponent>();
		componentMenu.ParentPart = ParentPart;
		componentMenu.ParentComponent = component;
		componentMenu.Folded = true;
		
		_componentMenus.Add(componentMenu);
		_componentMenuContainer.AddChild(componentMenu);
	}
	
	public override void _Process(double delta)
	{
		var verticalHeight = 50f + 25f; // Header Size + Extra Margin
		foreach (var menu in _componentMenus)
		{
			verticalHeight += menu.Size.Y;
		}

		Size = new Vector2(Size.X, verticalHeight);
	}
}
