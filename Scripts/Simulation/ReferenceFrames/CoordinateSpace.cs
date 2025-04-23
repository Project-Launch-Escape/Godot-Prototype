namespace GodotPrototype.Scripts.Simulation.ReferenceFrames;

public enum CoordinateSpace
{
    GalaxySpace = 0,
    StarSpace = 1,
    PlanetSpace = 2,
    MoonSpace = 3,
    //Other Coordinate spaces that do not follow the same nested structure:
    AbsoluteSpace = int.MaxValue-2,
    VesselSpace = int.MaxValue-1,
    RenderSpace = int.MaxValue
}