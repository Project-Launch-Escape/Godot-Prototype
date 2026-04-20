
using System.Text.Json.Serialization;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public class ConverterJSON : PartComponentJSON
{
    // No custom data yet

    public ConverterJSON(Converter tank, int index) : base(tank, index)
    {
        
    }
    
    [JsonConstructor]
    public ConverterJSON(int index, double baseMass) : base(index, baseMass)
    {
    }

    public override PartComponent InitializeComponent(PartComponent component)
    {
        return component;
    }
}