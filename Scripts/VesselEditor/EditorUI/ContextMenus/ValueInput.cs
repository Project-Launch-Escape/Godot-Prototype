using Godot;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class ValueInput : Control
{
	[Export] public Slider SliderInput;
	[Export] private SpinBox _textInput;
	[Export] private Label _valueText;
	[Export] private Label _maxValueText;

	[Export] private double _startingValue;
	[Export] private double _maxValue;
	[Export] private string _labelText;
	[Export] private string _units;
	[Export] private double _step;

	public event ValueChangedWithReference ValueChanged;

	public static ValueInput CreateValueInput(double value, double maxValue, string labelText, string units, double step)
	{
		var valueInput = ContextMenuController.ControllerNode.ValueInputScene.Instantiate<ValueInput>();
		valueInput.Initialize(value, maxValue, labelText, units, step);
		return valueInput;
	}

	public void Initialize(double value, double maxValue, string labelText, string units, double step)
	{
		_startingValue = value;
		_maxValue = maxValue;
		_labelText = labelText;
		_units = units;
		_step = step;
	}

	public override void _Ready()
	{
		_valueText.Text = _labelText;
		_maxValueText.Text = $"{_maxValue}{_units}";
		
		SliderInput.MaxValue = _maxValue;
		SliderInput.Value = _startingValue;

		_textInput.MaxValue = _maxValue;
		_textInput.Value = _startingValue;
		
		SliderInput.Step = _step;
		_textInput.Step = _step;
		
		SliderInput.ValueChanged += HandleSliderInput;
		_textInput.ValueChanged += HandleTextInput;
	}

	public void SetEditable(bool editable)
	{
		SliderInput.Editable = editable;
		_textInput.Editable = editable;
	}

	public void SetValueNoSignal(double newValue)
	{
		SliderInput.SetValueNoSignal(newValue);
		_textInput.SetValueNoSignal(newValue);
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
