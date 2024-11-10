using Godot;

namespace GodotPrototype.Scripts.UserInterface;

public partial class TransformTracker : Control
{
    
    [Export]
    public Camera3D CameraPosition;
    [Export]
    public Label TransformName;

    private Node3D _toTrack;

    public override void _Ready()
    {
        
        _toTrack = (Node3D)GetParent();
        TransformName.Text = _toTrack.Name;
    }

    public override void _Process(double delta)
    {
        var behind = CameraPosition.IsPositionBehind(_toTrack.GlobalPosition);
        Visible = !behind;
        if (behind) return;
        Position = CameraPosition.UnprojectPosition(_toTrack.GlobalPosition);
    }
}