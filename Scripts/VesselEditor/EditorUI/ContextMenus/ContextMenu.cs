using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class ContextMenu : Control
{
	[Export] private VBoxContainer _componentMenuContainer;
	[Export] private Button _xButton;
	[Export] private Label _partNameLabel;
	[Export] private Area2D _dragCollider;
	
	public VesselPart ParentPart;
	private List<Control> _componentMenus = [];

	private bool _hovered;
	private bool _dragging;

	public static Godot.Collections.Dictionary<ComponentType, PackedScene> ComponentMenuScenes => ContextMenuController.ControllerNode.ComponentMenus;

	public override void _Ready()
	{
		AddComponentMenu(null);
		foreach (var component in ParentPart.Components)
		{
			AddComponentMenu(component);
		}

		_partNameLabel.Text = ParentPart.PartName;
		
		_xButton.Pressed += OnXButtonPressed;
		ParentPart.TreeExiting += OnXButtonPressed;
		
		_dragCollider.MouseEntered += () => _hovered = true;
		_dragCollider.MouseExited += () => _hovered = _dragging;
	}

	private void AddComponentMenu(PartComponent component)
	{
		var componentMenu = ComponentMenuScenes[component?.ComponentType ?? ComponentType.Default].Instantiate<MenuComponent>();
		componentMenu.ParentPart = ParentPart;
		componentMenu.ParentComponent = component;
		
		_componentMenus.Add(componentMenu);
		_componentMenuContainer.AddChild(componentMenu);
	}

	private void OnXButtonPressed()
	{
		QueueFree();
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

	public override void _Input(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventMouseButton { ButtonIndex: MouseButton.Left} mouseButton:
				if (_hovered) _dragging = mouseButton.Pressed;
				break;
			case InputEventMouseMotion mouseMotion when _dragging:
				Position += mouseMotion.Relative;
				break;
		}
	}
}
