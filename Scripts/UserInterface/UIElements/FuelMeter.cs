using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class FuelMeter : TextureProgressBar
{
	public FuelSystem SystemToTrack;
	public FuelType FuelTypeToTrack;

	[Export] private Label _meterLabel;
	
	private static readonly PackedScene FuelMeterScene = GD.Load<PackedScene>("res://Scenes/UIElements/FuelMeter.tscn");
	
	public override void _Process(double delta)
	{
		Value = SystemToTrack.GetLevelOfFuelType(FuelTypeToTrack);
	}

	public static FuelMeter CreateFuelMeter(FuelSystem systemToTrack, FuelType fuelTypeToTrack)
	{
		var fuelMeter = (FuelMeter)FuelMeterScene.Instantiate().FindChild("Bar");
		fuelMeter.SystemToTrack = systemToTrack;
		fuelMeter.FuelTypeToTrack = fuelTypeToTrack;
		
		fuelMeter.MinValue = 0d;
		fuelMeter.MaxValue = systemToTrack.GetMaxOfFuelType(fuelTypeToTrack);

		fuelMeter._meterLabel.Text = fuelTypeToTrack.Name;
		fuelMeter.TintProgress = fuelTypeToTrack.Color;

		return fuelMeter;
	}
}
