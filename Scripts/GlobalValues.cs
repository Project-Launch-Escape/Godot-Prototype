using Godot;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
namespace GodotPrototype.Scripts;

public partial class GlobalValues : Node
{
    public static double TimeScale = 864000;
    public static double Time;
    public const double G = 0.00000000006674315f;
    public static readonly List<Celestial> AllCelestials = [];
    public static bool Paused;
    public static bool UIVisible = true;
    
    public const double Minute = 60;
    public const double Hour = Minute * 60;
    public const double Day = Hour * 24;
    public const double Year = Day * 365.2422d;
    
    public static void ReceiveCelestials(Celestial celestial)
    {
        if (!AllCelestials.Contains(celestial))
        {
            AllCelestials.Add(celestial);
        }
    }
    
    public override void _Process(double delta)
    {
        if (Paused) return;
        Time += delta * TimeScale;
    }

    public static string TimeToYearDayString(double time)
    {
        var days = time % Year;
        var years = (long)((time - days) / Year);
        return $"{years} yr, {days/Day:F1} d";
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey inputEventKey && inputEventKey.Pressed)
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
