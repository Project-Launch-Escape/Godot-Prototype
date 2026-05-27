using Godot;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

public partial class ManeuverMarker : OrbitMarker
{
	[Export] private ManeuverEditor _maneuverEditor;
	public Maneuver ParentManeuver;

	public override void _Ready()
	{
		AddToIconList();

		if (ParentOrbit.OrbitingObject is Celestial celestial)
		{
			Modulate = celestial.IconColor;
		}
	}

	public override void _Process(double delta)
	{
		_renderspacePosition = new RelativePosition(ParentManeuver.GetBurnStartPosition(), ParentManeuver.ParentOrbit.Primary)[CoordinateSpace.RenderSpace];
		Visible = GetVisibility();
		base._Process(delta);
	}

	public override void OnClick(InputEventMouseButton mouseEvent)
	{
		if (mouseEvent.Pressed || mouseEvent.ButtonIndex is not MouseButton.Left) return;
		if (_maneuverEditor == null) CreateManeuverEditor();
		_maneuverEditor!.Visible = true;
	}

	private void CreateManeuverEditor()
	{
		_maneuverEditor = ManeuverEditor.CreateEditor(ParentManeuver);
		_maneuverEditor.Position = Position + new Vector2(30, -30);
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
