using Godot;
using GodotPrototype.Scripts.VesselEditor;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/SnapPointIcon.png")]
public partial class SnapPoint : Node3D
{
	public bool Occupied => AttachedSnapPoint != null;
	[Export] public Area3D Collider;
	[Export] private CollisionShape3D _collisionShape;
	[Export] private MeshInstance3D _visual;
	public VesselPart ParentPart => (VesselPart)GetParent();

	public VesselPart AttachedPart => AttachedSnapPoint?.ParentPart;
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

	private void AttachTo(SnapPoint otherSnapPoint)
	{
		AttachedSnapPoint?.Unattatch();
		
		AttachedSnapPoint = otherSnapPoint;
		_collisionShape.Disabled = true;
		SetVisualState(SnapPointVisualState.Attached);
	}

	public static void AttachSnapPointTo(SnapPoint child, SnapPoint parent)
	{
		var parentPart = parent.ParentPart;
		var childPart = child.ParentPart;
		
		if (parentPart.GetAllDescendantParts().Contains(childPart)) return;

		if (childPart.IsInsideTree()) childPart.Reparent(parentPart);
		else parentPart.AddChild(childPart);

		parentPart.ParentTree.AppendTree(childPart.ParentTree);
		
		parent.AttachTo(child);
		child.AttachTo(parent);
	}

	public void Unattatch()
	{
		AttachedSnapPoint = null;
		_collisionShape.Disabled = false;
		SetVisualState(SnapPointVisualState.Normal);
	}

	public void SnapParentPartToSnapPoint(SnapPoint parentSnapPoint)
	{
		var childSnapPoint = this; var childPart = childSnapPoint.ParentPart; var parentPart = parentSnapPoint.ParentPart;
		
		if (childPart == parentPart || parentPart.GetAllDescendantParts().Contains(childPart)) return;

		childPart.Transform = GetSnappedTransform(parentSnapPoint);

		AttachSnapPointTo(childSnapPoint, parentSnapPoint);
	}

	public Transform3D GetSnappedTransform(SnapPoint parentSnapPoint)
	{
		//TODO: make this more generalized so it rotates along the axis perpendicular to the SnapPoint pointing direction
		var newBasis = parentSnapPoint.GlobalBasis * this.Basis;
		newBasis = newBasis.Rotated(newBasis.Z.Normalized(), Mathf.Pi);

		var tempBasis = ParentPart.Basis;
		ParentPart.GlobalBasis = newBasis;
		var newPosition = ParentPart.Position + parentSnapPoint.GlobalPosition - this.GlobalPosition;
		ParentPart.Basis = tempBasis;
		
		return new Transform3D(newBasis, newPosition);
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
