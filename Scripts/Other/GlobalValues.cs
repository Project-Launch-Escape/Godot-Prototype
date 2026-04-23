using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.Other;

public partial class GlobalValues : Node
{
    public static double TimeScale = 3600;
    public static double Time;
    
    public static bool Paused = true;
    public static bool UIVisible = true;
    
    public const double G = 0.00000000006674315;
    public const double Minute = 60;
    public const double Hour = Minute * 60; 
    public const double Day = Hour * 24;
    public const double Year = Day * 365; // Leap years are the bane of any programmer's existance

    public static readonly List<(string name, double multiplier)> SIPrefixes =
    [
        ("m",0.001), ("c",0.01), ("",1), ("k",1000), ("M",1000000), ("G",1000000000), ("T",1000000000000)
    ];

    public static Camera3D RenderSpaceCamera;
    public static Camera3D LocalSpaceCamera;

    [Export] private SceneType _currentScene;
    public static SceneType CurrentScene;

    [Export] private Camera3D _renderSpaceCamera;
    [Export] private Camera3D _localSpaceCamera;

    public static RelativePosition FOPosition = new();

    public override void _Ready()
    {
        CurrentScene = _currentScene;
        switch (_currentScene)
        {
            case SceneType.FlightScene:
                RenderSpaceCamera = _renderSpaceCamera;
                LocalSpaceCamera = _localSpaceCamera;
                break;
            case SceneType.VesselEditor:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public override void _Process(double delta)
    {
        if (Paused) return;
        Time += delta;
        Engine.TimeScale = TimeScale;
    }

    public static string TimeToYearDayString(double time)
    {
        var days = time % Year / Day;
        var years = (long)((time - Day * days) / Year);
        return $"year {years}, {days:F1} d";
    }
    public static string TimeToVerboseString(double time)
    {
        var years = (long)(time / Year);
        time -= years * Year;
        var days = (long)(time / Day);
        time -= days * Day;
        var hours = (long)(time / Hour);
        time -= hours * Hour;
        var minutes = (long)(time / Minute);
        time -= minutes * Minute;
        var seconds = (long)time;

        return $"Year {years}  Day {MinumumLength(days, 3)}\n Hour {MinumumLength(hours, 2)}:{MinumumLength(minutes, 2)}:{MinumumLength(seconds, 2)}";
        
        string MinumumLength(long number, long length)
        {
            var originalString = $"{number}";
            string leadingZeroes = "";
            for (int i = 0; i < length - originalString.Length; i++)
            {
                leadingZeroes += "0";
            }
            return leadingZeroes + originalString;
        }
    }
    public static (string prefixName, double prefixMultiplier) ScalarToHighestSIPrefix(double value)
    {
        value = Math.Abs(value);
        var highestPrefix = SIPrefixes[0];
        foreach (var prefix in SIPrefixes)
        {
            if (value >= prefix.multiplier && prefix.multiplier > highestPrefix.multiplier) highestPrefix = prefix;
        }
        return highestPrefix;
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