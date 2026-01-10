using Godot;
using GodotPrototype.Scripts.VesselEditor;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/SnapPointIcon.png")]
public partial class SnapPoint : Node3D
{
	//private static bool VisualsEnabled = false;
	
	public bool Occupied { get; private set; }
	[Export] public Area3D Collider;
	[Export] private CollisionShape3D _collisionShape;
	[Export] private MeshInstance3D _visual;
	public VesselPart Parent => (VesselPart)GetParentNode3D();

	public VesselPart AttachedPart;
	public SnapPoint AttachedSnapPoint;

	public SnapPointVisualState VisualState = SnapPointVisualState.Normal;
	
	private static event EventHandler<bool> OnVisualEnabledChanged;

	public static void SetVisualVisibility(bool visible)
	{
		OnVisualEnabledChanged?.Invoke(null, visible);
	}
	
	public override void _EnterTree()
	{
		if (_visual != null) OnVisualEnabledChanged += SetVisualEnabled;
	}

	public void AttachTo(SnapPoint otherSnapPoint)
	{
		AttachedSnapPoint?.Unattatch();
		
		AttachedPart = otherSnapPoint.Parent;
		AttachedSnapPoint = otherSnapPoint;
		Occupied = true;
		_collisionShape.Disabled = true;
		//otherSnapPoint.Parent.ChildParts.Add(Parent);
		SetVisualState(SnapPointVisualState.Attached);
	}

	public void Unattatch()
	{
		//AttachedPart.ChildParts.Remove(Parent);
		AttachedSnapPoint = null;
		AttachedPart = null;
		Occupied = false;
		_collisionShape.Disabled = false;
		SetVisualState(SnapPointVisualState.Normal);
	}

	private void SetVisualEnabled(object _, bool enabled) => Visible = enabled;

	public void SetVisualState(SnapPointVisualState state)
	{ // state = 0 is normal, state = 1 is hovered, state = 2 is selected
		_visual.Scale = state switch
		{
			SnapPointVisualState.Normal => Vector3.One,
			SnapPointVisualState.Hovered => 1.25f * Vector3.One,
			SnapPointVisualState.Selected => 1.5f * Vector3.One,
			_ => Vector3.One
		};
		_visual.Visible = state is not SnapPointVisualState.Attached;
		
		_visual.Position = new Vector3(0, 0.125f * _visual.Scale.Y, 0);
	}

	public override void _ExitTree()
	{
		if (_visual != null) OnVisualEnabledChanged -= SetVisualEnabled;
	}
	
	public enum SnapPointVisualState
	{
		Normal,
		Hovered,
		Selected,
		Attached
	}
}
