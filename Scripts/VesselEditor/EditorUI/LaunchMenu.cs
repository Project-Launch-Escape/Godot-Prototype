using Godot;
using GodotPrototype.Scripts.VesselEditor.FileInterfacing;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI;

public partial class LaunchMenu : Node
{
	[Export] private PopupMenu _launchMenu;
	
	[Export] private CheckBox _landedChecker;
	[Export] private LineEdit _celestialInput;

	[Export] private Control _launchSettingsPanel;


	private bool _selectingVessel;
	
	public override void _Ready()
	{
		_launchMenu.IdPressed += OnFileItemClicked;
		_launchSettingsPanel.Visible = false;
	}

	private void OnFileItemClicked(long id)
	{
		switch (id)
		{
			case 0:
				OnSelectLaunchVehicleSelected();
				break;
			case 1:
				OnOpenLaunchConfigSelected();
				break;
			default:
				return;
		}
	}

	private void OnSelectLaunchVehicleSelected()
	{
		_selectingVessel = true;
	}

	private void SelectVehicle()
	{
		var part = Editor.GetMouseHoveredPart();
		if (part == null) return;

		var launchFile = new LaunchFile(part)
		{
			StartingCelestial = _celestialInput.Text,
			StartOnSurface = _landedChecker.ButtonPressed
		};
		VesselFileTools.SaveLaunchFile(launchFile);
		_selectingVessel = false;
	}
	
	private void OnOpenLaunchConfigSelected()
	{
		_launchSettingsPanel.Visible = true;
	}
	public override void _Input(InputEvent inputEvent)
	{
		if (_selectingVessel && inputEvent is InputEventMouseButton {ButtonIndex: MouseButton.Middle})
		{
			SelectVehicle();
		}
	}
}
