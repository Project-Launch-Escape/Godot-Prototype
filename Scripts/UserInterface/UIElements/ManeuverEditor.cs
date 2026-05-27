using Godot;
using GodotPrototype.Scripts.Simulation.Physics;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class ManeuverEditor : Draggable
{
	public Maneuver ParentManeuver;

	public static ManeuverEditor CreateEditor(Maneuver parentManeuver)
	{
		var editor = FlightUIController.ControllerNode.ManeuverEditorScene.Instantiate<ManeuverEditor>();
		editor.ParentManeuver = parentManeuver;
		FlightUIController.ControllerNode.AddChild(editor);
		return editor;
	}
}
