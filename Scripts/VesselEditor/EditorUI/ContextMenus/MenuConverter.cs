using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class MenuConverter : MenuComponent
{
	private Converter ParentConverter => (Converter)ParentComponent;
	
	[Export] private Label _inputs;
	[Export] private Label _outputs;
	[Export] private ValueInput _rateInput;

	public override void _Ready()
	{
		_rateInput.SetValueNoSignal(ParentConverter.Rate * 100);
		_rateInput.ValueChanged += OnRateChanged;
	}
	public override void _Process(double delta)
	{
		if (Folded) return;

		string inputs = "  Inputs:";
		foreach (var (fuelType, fuelRate) in ParentConverter.Inputs)
		{
			var massRate = -fuelRate * fuelType.Mass * ParentConverter.Rate * 1000; //Value in g/s
			var prefix = GlobalValues.ScalarToHighestSIPrefix(massRate);
			inputs += $"\n    {fuelType.Name}: {massRate / prefix.prefixMultiplier:F}{prefix.prefixName}g/s";
		}
		_inputs.Text = inputs;
		string outputs = "  Outputs:";
		foreach (var (fuelType, fuelRate) in ParentConverter.Outputs)
		{
			var massRate = fuelRate * fuelType.Mass * ParentConverter.Rate * 1000; //Value in g/s
			var prefix = GlobalValues.ScalarToHighestSIPrefix(massRate);
			outputs += $"\n    {fuelType.Name}: {massRate / prefix.prefixMultiplier:F}{prefix.prefixName}g/s";
		}
		_outputs.Text = outputs;
	}

	private void OnRateChanged(object valueInput, double rate)
	{
		ParentConverter.Rate = rate / 100; // Percent to ratio
	}
}
