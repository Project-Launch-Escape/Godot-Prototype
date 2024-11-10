using Godot;

namespace GodotPrototype.Scripts.UserInterface;

public partial class TransformTracker : Control
{

    [Export]
    public Camera3D CameraPosition;
    [Export]
    public Label TransformName;

    private bool _visible = true;
        
    private Node3D _toTrack;

    public override void _Ready()
    {

        _toTrack = (Node3D)GetParent();
        TransformName.Text = _toTrack.Name;

    }

    public override void _Process(double delta)
    {
        var behind = CameraPosition.IsPositionBehind(_toTrack.GlobalPosition);
        Visible = !behind && _visible;
        if (behind) return;
        Position = CameraPosition.UnprojectPosition(_toTrack.GlobalPosition);
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey inputEventKey)
        {
            if (inputEventKey.Pressed && inputEventKey.Keycode == Key.M)
            {
                _visible = !_visible;
            }
        }
    }
}
