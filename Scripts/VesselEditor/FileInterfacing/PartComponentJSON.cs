
using System.Text.Json.Serialization;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "ComponentType")]
[JsonDerivedType(typeof(FuelTankJSON), "FuelTank")]
[JsonDerivedType(typeof(RocketEngineJSON), "Engine")]
public abstract class PartComponentJSON
{
    public int Index; // Index in the array of PartComponents
    public double BaseMass;
    
    public PartComponentJSON(PartComponent component, int index)
    {
        Index = index;
        BaseMass = component.BaseMass;
    }
    
    [JsonConstructor]
    public PartComponentJSON(int index, double baseMass)
    {
        Index = index;
        BaseMass = baseMass;
    }

    public static PartComponentJSON ComponentToJsonCompatible(PartComponent component, int index)
    {
        return component switch
        {
            FuelTank tank => new FuelTankJSON(tank, index),
            RocketEngine rocket => new RocketEngineJSON(rocket, index),
            not null => throw new NotImplementedException(),
            _ => throw new Exception("PartComponent is null!")
        };
    }

    public abstract PartComponent InitializeComponent(PartComponent component);
}