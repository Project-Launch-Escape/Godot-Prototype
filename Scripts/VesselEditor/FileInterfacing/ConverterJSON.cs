
using System.Text.Json.Serialization;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public class ConverterJSON : PartComponentJSON
{
    public double Rate;

    public ConverterJSON(Converter tank, int index) : base(tank, index)
    {
        Rate = tank.Rate;
    }
    
    [JsonConstructor]
    public ConverterJSON(int index, double baseMass, double rate) : base(index, baseMass)
    {
        Rate = rate;
    }

    public override PartComponent InitializeComponent(PartComponent component)
    {
        var converter = (Converter)component;
        converter.Rate = Rate;
        return component;
    }
}