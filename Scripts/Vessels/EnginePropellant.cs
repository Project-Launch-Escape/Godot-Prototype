using Godot;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass]
public partial class EnginePropellant : Resource
{
    [Export] public FuelType Fuel;
    [Export] public double Ratio; // Ratio of fuel that this propellant takes.
                                  // A ratio of '1' means this propellant takes 100% of the engine's fuel drain
    
}