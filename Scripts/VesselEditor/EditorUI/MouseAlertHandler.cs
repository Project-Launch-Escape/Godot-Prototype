using Godot;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI;

public partial class MouseAlertHandler : VBoxContainer
{
	private static MouseAlertHandler _handler;
	[Export] private PackedScene _alertScene;
	private readonly Dictionary<string, MouseAlert> _alerts = [];

	public override void _Ready()
	{
		if (_handler != null) GD.PrintErr("Multiple MouseAlertHandler Nodes!");
		_handler = this;
	}
	
	public static void CreateMouseAlert(string text, float time, string identifier = null)
	{
		var alert = _handler._alertScene.Instantiate<MouseAlert>();
		alert.Initialize(text, time);
		_handler.AddChild(alert);

		if (identifier == null) return;
		_handler._alerts.Add(identifier, alert);
		alert.TreeExiting += () => _handler._alerts.Remove(identifier);
	}

	public static void RemoveMouseAlert(string identifier)
	{
		if (_handler._alerts.TryGetValue(identifier, out var alert)) alert.Remove();
	}

	public override void _Process(double delta)
	{
		var offset = new Vector2(25,25);
		Position = GetViewport().GetMousePosition() + offset;
	}
}
