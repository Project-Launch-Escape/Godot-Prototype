using Godot;

namespace GodotPrototype.Scripts.UserInterface;

[GlobalClass, Icon("res://Resources/Icons/XButtonIcon.png")]
public partial class XButton : Button
{
	[Export] public XButtonMode CloseMode;
	[Export] private Control _toHide;
	
	public enum XButtonMode
	{
		Hide,
		Delete
	}

	public override void _EnterTree()
	{
		Pressed += OnXButtonPress;
	}

	public void OnXButtonPress()
	{
		switch (CloseMode)
		{
			case XButtonMode.Delete:
				_toHide.QueueFree();
				break;
			case XButtonMode.Hide:
				_toHide.Visible = false;
				break;
		}
	}
}
