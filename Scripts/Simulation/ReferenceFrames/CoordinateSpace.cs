namespace GodotPrototype.Scripts.Simulation.ReferenceFrames;

public enum CoordinateSpace
{
    RenderSpace = 0,
    GalaxySpace = 1,
    StarSpace = 2,
    PlanetSpace = 3,
    MoonSpace = 4
}

public static class Extensions
{
    public static CoordinateSpace Increment(this CoordinateSpace coordLayer)
    {
        return (CoordinateSpace)((int)coordLayer + 1);
    }
}
