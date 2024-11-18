using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
namespace GodotPrototype.Scripts;

public partial class GlobalValues : Node
{
    public static double TimeScale = 864000;
    public static double Time;
    public const float G = 0.000000000066743f;
    public static readonly float[] Units = [1f, 281474976710656f, 4294967296f, 65536f];
    public static readonly List<CelestialScript> AllCelestials = [];
    public static bool Paused = false;
    
    public static float GetRefConversionFactor(CoordinateSpace startingLayer, CoordinateSpace conversionLayer)
    {
        if ((int)startingLayer > Units.Length-1)
        {
            startingLayer = (CoordinateSpace)(Units.Length-1);
        }
        if ((int)conversionLayer > Units.Length-1)
        {
            conversionLayer = (CoordinateSpace)(Units.Length-1);
        }
        return Units[(int)startingLayer] / Units[(int)conversionLayer];
    }
    
    public static void ReceiveCelestials(CelestialScript celestial)
    {
        if (!AllCelestials.Contains(celestial))
        {
            AllCelestials.Add(celestial);
        }
    }
    
    public override void _Ready()
    {
    }
    
    public override void _Process(double delta)
    {
        if (Paused) return;
        var dt = Convert.ToSingle(delta);
        Time += dt * TimeScale;
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
