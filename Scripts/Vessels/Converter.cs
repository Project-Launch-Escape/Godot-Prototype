using Godot;
using GodotPrototype.Scripts.Other;

namespace GodotPrototype.Scripts.Vessels;
[GlobalClass]
public partial class Converter : PartComponent
{
	[Export] public Godot.Collections.Dictionary<FuelType, double> Inputs; // Values in units/s
	[Export] public Godot.Collections.Dictionary<FuelType, double> Outputs; // Values in units/s
	/// Value from 0-1 describing the rate at which this converter is running. 0 is off, and 1 is 100% production
	[Export] public double Rate = 1;

	public override void _Process(double delta)
	{
		if (GlobalValues.CurrentScene is SceneType.VesselEditor) return;
		if (Rate <= 0) return;
		
		var fuelDeltas = new Dictionary<FuelType, double>();
		foreach (var (fuelType, fuelDelta) in Inputs)
		{
			fuelDeltas.Add(fuelType, fuelDelta * delta * Rate);
		}
		ConnectedFuelSystem.AddFuelRequest(fuelDeltas, OnFuelDrain);
	}

	public void OnFuelDrain(Dictionary<FuelType, double> fuelDeltas) // TODO: Fix this clusterfuck
	{
		var totalDrain = 0d;
		foreach (var (_, fuelDrain) in fuelDeltas)
		{
			totalDrain += fuelDrain;
		}
		var totalInput = 0d;
		foreach (var (_, fuelDrain) in Inputs)
		{
			totalInput += fuelDrain;
		}
		var amount = totalDrain / totalInput;
		
		var fuelFillDeltas = new Dictionary<FuelType, double>();
		foreach (var (fuelType, fuelDelta) in Outputs)
		{
			fuelFillDeltas.Add(fuelType, fuelDelta * amount);
		}
		ConnectedFuelSystem.AddFuelRequest(fuelFillDeltas, null);
	}
}
