using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class ThrottleGauge : VSlider
{
	[Export] private Label _throttleLevelText;
	public override void _Process(double delta)
	{
		if (Vessel.ActiveVessel == null) return;
		Value = Vessel.ActiveVessel.Throttle;
		
		_throttleLevelText.Text = $"{Value * 100:F0}%";
		_throttleLevelText.Position = new Vector2(_throttleLevelText.Position.X, (1 - (float)Value) * Size.Y);
	}

	public override void _ValueChanged(double newValue)
	{
		if (Vessel.ActiveVessel == null) return;
		Vessel.ActiveVessel.Throttle = newValue;
	}
}
