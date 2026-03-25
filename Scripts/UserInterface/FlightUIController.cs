using Godot;
using GodotPrototype.Scripts.UserInterface.UIElements;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface;

public partial class FlightUIController : Node
{
	[Export] private Container _fuelMeterContainer;
	private readonly List<TextureProgressBar> _fuelMeters = [];
	
	public static HoverIcon HoveredIcon;
	public static List<HoverIcon> LockedIcons = [];
	private const float HoverDistance = 25f;
	
	public override void _Ready()
	{
		InitializeFuelMeters();
	}

	private void InitializeFuelMeters()
	{
		var activeFuelSystem = Vessel.ActiveVessel.FuelSystems[0];
		var activeVesselFuelTypes = activeFuelSystem.GetContainedFuelTypes();
		foreach (var fuelType in activeVesselFuelTypes)
		{
			var fuelMeter = FuelMeter.CreateFuelMeter(activeFuelSystem, fuelType);
			_fuelMeterContainer.AddChild(fuelMeter.GetParent());
			_fuelMeters.Add(fuelMeter);
		}
	}

	public override void _Process(double delta)
	{
		HandleHoverIcons();
	}

	public void HandleHoverIcons()
	{
		var mousePos = GetViewport().GetMousePosition();
		
		HoverIcon closestIcon = null;
		var closestIconDist = double.MaxValue;
		foreach (var hoverIcon in HoverIcon.HoverIcons)
		{
			if (!LockedIcons.Contains(hoverIcon)) hoverIcon.IsHovered = false;
			var iconDist = hoverIcon.GlobalPosition.DistanceTo(mousePos);
			
			if (iconDist > closestIconDist || !hoverIcon.Visible) continue;
			if (Input.IsMouseButtonPressed(MouseButton.Right) && hoverIcon != HoveredIcon) continue;
			
			closestIconDist = iconDist;
			closestIcon = hoverIcon;
		}
		if (closestIconDist >= HoverDistance || DisplayServer.MouseGetMode() is not DisplayServer.MouseMode.Visible) closestIcon = null;

		if (closestIcon != HoveredIcon)
		{
			if (!LockedIcons.Contains(HoveredIcon)) HoveredIcon?.OnHoverExit();
			closestIcon?.OnHoverEnter();
		}
		
		foreach (var icon in LockedIcons)
		{
			icon.WhileHovered();
		}
		if (!LockedIcons.Contains(closestIcon)) closestIcon?.WhileHovered();

		if (closestIcon is null)
		{
			HoveredIcon = null;
			return;
		}

		closestIcon.IsHovered = true;

		HoveredIcon = closestIcon;
	}

	public override void _Input(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventMouseButton {ButtonIndex: MouseButton.Right, Pressed: false}:
			{
				if (HoveredIcon is null) break;
				if (!LockedIcons.Contains(HoveredIcon)) // Attempts remove. If in list, removes and returns false, if not in list, returns true
				{
					LockedIcons.Add(HoveredIcon);
					HoveredIcon.IsHovered = true;
					HoveredIcon.IsLocked = true;
				}
				else
				{
					LockedIcons.Remove(HoveredIcon);
					HoveredIcon.IsLocked = false;
				}
				break;
			}
		}
	}
}
