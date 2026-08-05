using Godot;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.UserInterface.UIElements;
using GodotPrototype.Scripts.VesselEditor;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.Other;

public partial class Initializer : Node
{
	[Export] private Node _vesselParent;

	public override void _Ready()
	{
		if (GlobalValues.CurrentScene is SceneType.FlightScene) InitializeFlightScene();
	}

	public void InitializeFlightScene()
	{
		InitializeVessel();
	}

	public void InitializeVessel()
	{
		FuelType.InitializeFuelTypes();
		Editor.InitializeParts();
		SnapPoint.SetGlobalVisibility(false);

		var launchVessel = Vessel.CreateVesselFromLaunchFile();
		_vesselParent.AddChild(launchVessel);
		launchVessel.MakeActiveVessel();
		FlightUIController.ControllerNode.InitializeFuelMeters();
	}
}
