using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface;

public partial class FlightUIController : Node
{
	[Export] private Container _fuelMeterContainer;
	private List<TextureProgressBar> _fuelMeters = [];
	
	
	public override void _Ready()
	{
		InitializeFuelMeters();
	}

	public void InitializeFuelMeters()
	{
		var activeFuelSystem = Vessel.ActiveVessel.FuelSystems[0];
		var activeVesselFuelTypes = activeFuelSystem.GetContainedFuelTypes();
		foreach (var fuelType in activeVesselFuelTypes)
		{
			var fuelMeter = FuelMeter.CreateFuelMeter(activeFuelSystem, fuelType);
			_fuelMeterContainer.AddChild(fuelMeter);
			_fuelMeters.Add(fuelMeter);
		}
	}
}
