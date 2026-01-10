using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public partial class MenuDefault : MenuComponent
{
	[Export] private Label _mass;
	[Export] private Label _snapPoints;
	[Export] private Label _children;
	[Export] private Label _rootPart;

	public override void _Process(double delta)
	{
		if (Folded) return;

		int occupied = ParentPart.SnapPoints.Count(snapPoint => snapPoint.Occupied);

		_mass.Text = $"  Mass: {ParentPart.Mass}kg";
		_snapPoints.Text = $"  Snap Points Occupied: {occupied} / {ParentPart.SnapPoints.Length}";
		_children.Text = $"  Child Parts: {ParentPart.ChildParts.Count}";
		_rootPart.Text = $"  Is Root Part:  {ParentPart.IsRootPart}";
	}
}
