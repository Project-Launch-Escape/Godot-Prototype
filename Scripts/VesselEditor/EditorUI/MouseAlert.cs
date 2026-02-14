using Godot;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI;

public partial class MouseAlert : Control
{
	[Export] private Label _text;
	public float TimeRemaining;
	private const float FadeOutTime = 1;
	
	public override void _Process(double delta)
	{
		TimeRemaining -= (float)delta;
		
		
		Modulate = new Color(1, 1, 1, Math.Min(1, TimeRemaining / FadeOutTime));
		if (TimeRemaining <= 0)
		{
			QueueFree();
		}
	}

	public void Remove()
	{
		TimeRemaining = FadeOutTime;
	}

	public void Initialize(string text, float time)
	{
		_text.Text = text;
		TimeRemaining = time;
	}
}
