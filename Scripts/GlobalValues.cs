using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
namespace GodotPrototype.Scripts;

public partial class GlobalValues : Node
{
    public static double TimeScale = 864000;
    public static double Time;
    public const double G = 0.00000000006674315f;
    public static readonly List<CelestialScript> AllCelestials = [];
    public static bool Paused;
    
    public static void ReceiveCelestials(CelestialScript celestial)
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
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey inputEventKey)
        {
            if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Space)
            {
                Paused = !Paused;
            }
            if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Comma)
            {
                TimeScale /= 1.5f;
            }
            if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Period)
            {
                TimeScale *= 1.5f;
            }
        }
    }
}
