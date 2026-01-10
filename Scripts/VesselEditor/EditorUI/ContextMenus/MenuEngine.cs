using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class MenuEngine : MenuComponent
{
	private RocketEngine ParentEngine => (RocketEngine)ParentComponent;
	
	[Export] private Label _thrust;
	[Export] private Label _twr;
	[Export] private Label _isp;
	[Export] private Label _fuelTypes;

	public override void _Process(double delta)
	{
		if (Folded) return;
		
		var typesSum = ParentEngine.Propellants.Aggregate("", (current, propellant) => current + propellant.Fuel.Name + "   ");

		_thrust.Text = $"  Thrust: {ParentEngine.MaxThrust / 1000}kN";
		_twr.Text = $"  TWR: {ParentEngine.MaxThrust / (ParentPart.Mass * 9.81) :N1}";
		_isp.Text = $"  ISP: {ParentEngine.Isp}m/s ({ParentEngine.Isp / 9.81 :N1}s)";
		_fuelTypes.Text = $"  Fuel Types:  {typesSum}";
	}
}
