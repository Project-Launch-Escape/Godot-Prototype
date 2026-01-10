using Godot;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class ValueInput : Control
{
	[Export] public Slider SliderInput;
	[Export] private SpinBox _textInput;
	[Export] private Label _valueText;
	[Export] private Label _maxValueText;

	private double _maxValue;

	public event ValueChangedWithReference ValueChanged;

	public static ValueInput CreateValueInput(double value, double maxValue, string labelText)
	{
		var valueInput = ContextMenuController.ControllerNode.ValueInputScene.Instantiate<ValueInput>();
		valueInput.Initialize(value, maxValue, labelText);
		return valueInput;
	}

	public void Initialize(double value, double maxValue, string labelText)
	{
		_maxValue = maxValue;
		_valueText.Text = labelText;
		
		SliderInput.MaxValue = maxValue;
		SliderInput.Value = value;

		_textInput.MaxValue = maxValue;
		_textInput.Value = value;
		
		SliderInput.ValueChanged += HandleSliderInput;
		_textInput.ValueChanged += HandleTextInput;
	}

	private void HandleTextInput(double newValue)
	{
		SliderInput.SetValueNoSignal(newValue);
		ValueChanged?.Invoke(this, newValue);
	}
	private void HandleSliderInput(double newValue)
	{
		_textInput.SetValueNoSignal(newValue);
		ValueChanged?.Invoke(this, newValue);
	}
}

public delegate void ValueChangedWithReference(object valueInput, double value);
