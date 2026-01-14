
using System.Text.Json.Serialization;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public class RocketEngineJSON : PartComponentJSON
{
    // No custom data yet

    public RocketEngineJSON(RocketEngine tank, int index) : base(tank, index)
    {
        
    }
    
    [JsonConstructor]
    public RocketEngineJSON(int index, double baseMass) : base(index, baseMass)
    {
    }

    public override PartComponent InitializeComponent(PartComponent component)
    {
        return component;
    }
}