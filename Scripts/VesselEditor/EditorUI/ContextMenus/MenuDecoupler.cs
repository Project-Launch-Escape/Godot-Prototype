using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class MenuDecoupler : MenuComponent
{
	[Export] private ContextButton _decoupleButton;
	private Decoupler ParentDecoupler => (Decoupler)ParentComponent;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_decoupleButton.BindAction(ParentDecoupler.Decouple);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
