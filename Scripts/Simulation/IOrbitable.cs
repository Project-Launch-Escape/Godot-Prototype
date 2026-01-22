using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.Simulation;

public interface IOrbiter
{
    public string Name => this is Celestial celestial? celestial.Name : ((Vessel)this).Name;

    public bool IsType<T>() => this is T;
}