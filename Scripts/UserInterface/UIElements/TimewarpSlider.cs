using Godot;
using GodotPrototype.Scripts.Other;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class TimewarpSlider : HSlider
{
	public static TimewarpSlider SliderNode;
	private const double WarpRateBase = 1.41421356237d; // sqrt(2)

	[Export] private Label _timeWarpLabelString;
	[Export] private Label _timeWarpLabelInteger;
	[Export] private Label _currentTimeLabel;
	
	public override void _Ready()
	{
		SliderNode = this;
	}

	public override void _Process(double delta)
	{
		//if (Math.Abs(GlobalValues.TimeScale - Value) > 0.0001) Value = GlobalValues.TimeScale;
		GlobalValues.TimeScale = Math.Pow(WarpRateBase, Value);

		_timeWarpLabelString.Text = GlobalValues.TimeScale switch
		{
			< GlobalValues.Minute => $"{GlobalValues.TimeScale:F1} s/s",
			< GlobalValues.Hour => $"{GlobalValues.TimeScale / GlobalValues.Minute:F1} min/s",
			< GlobalValues.Day => $"{GlobalValues.TimeScale / GlobalValues.Hour:F1} hr/s",
			< GlobalValues.Year => $"{GlobalValues.TimeScale / GlobalValues.Day:F1} day/s",
			_ => $"{GlobalValues.TimeScale / GlobalValues.Year:F1} yr/s"
		};
		_timeWarpLabelInteger.Text = $"{GlobalValues.TimeScale:N1}x";

		_currentTimeLabel.Text = GlobalValues.TimeToVerboseString(GlobalValues.Time);
	}

	public void OverrideWarpSpeed(double newWarpRate)
	{
		Value = Math.Log(newWarpRate, WarpRateBase);
		GlobalValues.TimeScale = newWarpRate;
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is not InputEventKey { Pressed: true } inputEventKey) return;
		switch (inputEventKey.Keycode)
		{
			case Key.Comma:
				Value -= 2 * PLEInput.GetActiveModifier(2);
				break;
			case Key.Period:
				Value += 2 * PLEInput.GetActiveModifier(2);
				break;
			case Key.Slash:
				OverrideWarpSpeed(1);
				break;
		}
	}
}
