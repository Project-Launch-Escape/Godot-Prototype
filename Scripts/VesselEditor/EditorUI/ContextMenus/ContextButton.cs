using Godot;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class ContextButton : Control
{
	[Export] public Button ActionButton;

	public static ContextButton CreateButton(string text, Action action)
	{
		var cb = ContextMenuController.ControllerNode.ContextButtonScene.Instantiate<ContextButton>();
		
		cb.ActionButton.Text = text;
		cb.ActionButton.Pressed += action;
		return cb;
	}

	public void BindAction(Action action)
	{
		ActionButton.Pressed += action;
	}
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
