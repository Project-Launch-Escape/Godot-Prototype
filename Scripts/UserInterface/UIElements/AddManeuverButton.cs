using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class AddManeuverButton : Button
{
	public override void _Ready()
	{
		Pressed += () => Vessel.ActiveVessel.AddManeuver();
	}
}
