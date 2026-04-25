using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class MenuFuelTank : MenuComponent
{
	private FuelTank ParentTank => (FuelTank)ParentComponent;
	
	[Export] private Label _fuelLevels;
	[Export] private Label _fuelMax;
	[Export] private Label _fuelMass;
	private Dictionary<FuelType, ValueInput> _fuelLevelInputs = [];

	public override void _Ready()
	{
		InitializeFuelLevelInputs();
	}

	private void InitializeFuelLevelInputs()
	{
		foreach (var fuelType in ParentTank.FuelTypes)
		{
			var currentValue = ParentTank.FuelLevels[fuelType];
			var maxValue = ParentTank.MaxFuelLevels[fuelType];
			
			var valueInput = ValueInput.CreateValueInput(currentValue, maxValue, fuelType.Name + ":", "U", 0.1);
			valueInput.ValueChanged += OnValueChanged;

			valueInput.SliderInput.Modulate = fuelType.Color;
			
			_fuelLevelInputs.Add(fuelType, valueInput);
			ComponentMenuContainer.AddChild(valueInput);
			if (GlobalValues.CurrentScene is SceneType.FlightScene) valueInput.SetEditable(false);
		}
	}
	private void OnValueChanged(object valueInput, double value)
	{
		foreach (var fuelLevelInput in _fuelLevelInputs)
		{
			if (valueInput == fuelLevelInput.Value)
			{
				ParentTank.SetFuelAmount(value, fuelLevelInput.Key);
			}
		}
		
	}

	public override void _Process(double delta)
	{
		if (Folded) return;
		if (GlobalValues.CurrentScene is SceneType.FlightScene) FlightSceneProcess();
		
		var volumeSum = 0d;
		var maxSum = ParentTank.MaxFuelLevels.Sum(level => level.Value * level.Key.Volume);
		var massSum = 0d;
		foreach (var level in ParentTank.FuelLevels)
		{
			volumeSum += level.Value * level.Key.Volume;
			massSum += level.Value * level.Key.Mass;
		}
		
		_fuelLevels.Text = $"  Total Fuel: {volumeSum:F3}L";
		_fuelMax.Text = $"  Total Fuel Max: {maxSum:F3}L";
		_fuelMass.Text = $"  Fuel Mass: {massSum:F3}kg";
	}

	private void FlightSceneProcess()
	{
		foreach (var (fuelType, valueInput) in _fuelLevelInputs)
		{
			valueInput.SetValueNoSignal(ParentTank.FuelLevels[fuelType]);
		}
	}
}
