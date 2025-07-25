using Godot;

namespace GodotPrototype.Scripts.Other;

public partial class GlobalValues : Node
{
    public static double TimeScale = 3600;
    public static double Time;
    
    public static bool Paused = true;
    public static bool UIVisible = true;
    
    public const double G = 0.00000000006674315f;
    public const double Minute = 60;
    public const double Hour = Minute * 60; 
    public const double Day = Hour * 24;
    public const double Year = Day * 365.2422;

    public static Camera3D RenderSpaceCamera;
    public static Camera3D LocalSpaceCamera;
    
    [Export] private Camera3D _renderSpaceCamera;
    [Export] private Camera3D _localSpaceCamera;

    public override void _Ready()
    {
        RenderSpaceCamera = _renderSpaceCamera;
        LocalSpaceCamera = _localSpaceCamera;
    }
    
    public override void _Process(double delta)
    {
        if (Paused) return;
        Time += delta * TimeScale;
    }

    public static string TimeToYearDayString(double time)
    {
        var days = time % Year / Day;
        var years = (long)((time - Day * days) / Year);
        return $"{years} yr, {days:F1} d";
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true } inputEventKey)
        {
            switch (inputEventKey.Keycode)
            {
                case Key.Space:
                    Paused = !Paused;
                    break;
                case Key.Comma:
                    TimeScale /= 1.5;
                    break;
                case Key.Period:
                    TimeScale *= 1.5;
                    break;
                case Key.Slash:
                    TimeScale = 1;
                    break;
                case Key.M:
                    UIVisible = !UIVisible;
                    break;
            }
        }
    }
}
public enum PhysicsType
{
    Kepler = 0,
    Newton = 1
}