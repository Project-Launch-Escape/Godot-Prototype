using Godot;
using GodotPrototype.Scripts.Simulation.DoublePrecision;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/EngineIcon.png")]
public partial class RocketEngine : PartComponent
{
    [Export] public double MaxThrust;
    [Export] public double Isp; // Measured in Meters/second instead of seconds for ease of computation. And m/s is just better ok
    [Export] public EnginePropellant[] Propellants;

    public void FireEngine(double throttle, double fireTime)
    {
        bool canFire = Propellants.All(propellant => ConnectedFuelSystem.HasFuelType(propellant.Fuel));
        if (!canFire) return;

        var fireImpulse = MaxThrust * throttle * fireTime;
        foreach (var propellant in Propellants)
        {
            ConnectedFuelSystem.ChangeFuelAmount(-fireImpulse * propellant.Ratio / (Isp * propellant.Fuel.Mass), propellant.Fuel);
        }
        ParentVessel.AddImpulse(fireImpulse * (Vector3d)GlobalBasis.Y.Normalized());
    }
}