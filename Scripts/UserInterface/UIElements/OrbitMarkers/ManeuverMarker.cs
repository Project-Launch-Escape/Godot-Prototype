using Godot;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

public partial class ManeuverMarker : OrbitMarker
{
	[Export] public ManeuverEditor EditorNode;
	public Maneuver ParentManeuver;

	public void Initialize(Maneuver parentManeuver)
	{
		ParentManeuver = parentManeuver;
		AddToIconList();
		CreateManeuverEditor();
	}

	public Vector3d GetRenderSpacePosition() => new RelativePosition(ParentManeuver.GetBurnStartPosition(), ParentManeuver.ParentOrbit.Primary)[CoordinateSpace.RenderSpace];
	public override void _Process(double delta)
	{
		_renderspacePosition = GetRenderSpacePosition();
		Visible = GetVisibility();
		base._Process(delta);
	}

	public override void OnClick(InputEventMouseButton mouseEvent)
	{
		if (mouseEvent.Pressed || mouseEvent.ButtonIndex is not MouseButton.Left) return;
		if (EditorNode == null) CreateManeuverEditor();
		EditorNode!.Visible = true;
	}

	private void CreateManeuverEditor()
	{
		EditorNode = ManeuverEditor.CreateEditor(ParentManeuver);
		EditorNode.Position = Camera.UnprojectPosition((Vector3)GetRenderSpacePosition()) + new Vector2(30, -30);
	}

	public override void OnHoverEnter()
	{
		Scale = Vector2.One * 1.25f;
	}

	public override void OnHoverExit()
	{
		Scale = Vector2.One * 1f;
	}
}
