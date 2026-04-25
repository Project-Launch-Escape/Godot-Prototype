using Godot;

namespace GodotPrototype.Scripts.UserInterface;

[GlobalClass, Icon("res://Resources/Icons/DragIcon.png")]
public partial class Draggable : Control
{
	[Export] protected CollisionObject2D _mouseCollider;
	protected bool _hovered;
	protected bool _dragging;

	public override void _EnterTree()
	{
		// TODO: Make custom mouse collision code so that it works in
		_mouseCollider.InputPickable = true;
		
		_mouseCollider.MouseEntered += () => _hovered = true;
		_mouseCollider.MouseExited += () => _hovered = _dragging;
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		//GD.Print($"Hovered: {_hovered}, Dragging: {_dragging}");
		switch (inputEvent)
		{
			case InputEventMouseButton { ButtonIndex: MouseButton.Left} mouseButton:
				if (_hovered) _dragging = mouseButton.Pressed;
				break;
			case InputEventMouseMotion mouseMotion when _dragging:
				Position += mouseMotion.Relative; //TODO: Add screen edge collision
				break;
		}
	}
}
