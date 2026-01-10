using Godot;
using GodotPrototype.Scripts.Other;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class PauseButton : TextureButton
{
    [Export] private Texture2D _playIcon;
    [Export] private Texture2D _pauseIcon; 
    
    public override void _Pressed()
    {
        GlobalValues.Paused = !GlobalValues.Paused;
    }

    public override void _Process(double delta)
    {
        TextureNormal = GlobalValues.Paused ? _pauseIcon : _playIcon;
    }
}