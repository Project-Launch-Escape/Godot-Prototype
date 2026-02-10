using Godot;

namespace GodotPrototype.Scripts.UserInterface;

[GlobalClass, Icon("res://Resources/Icons/DragIcon.png")]
public partial class Draggable : Control
{
	[Export] protected CollisionObject2D MouseCollider;
	protected bool Hovered;
	protected bool Dragging;

	public override void _EnterTree()
	{
		MouseCollider.InputPickable = true;
		
		MouseCollider.MouseEntered += () => Hovered = true;
		MouseCollider.MouseExited += () => Hovered = Dragging;
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventMouseButton { ButtonIndex: MouseButton.Left} mouseButton:
				if (Hovered) Dragging = mouseButton.Pressed;
				break;
			case InputEventMouseMotion mouseMotion when Dragging:
				Position += mouseMotion.Relative; //TODO: Add screen edge collision
				break;
		}
	}
}
