using Godot;

namespace GodotPrototype.Scripts.Simulation.DoublePrecision;

public readonly struct Range
{
    public readonly double MinValue;
    public readonly double MaxValue;
    
    public readonly bool Inclusive;

    public Range()
    {
        MinValue = double.NegativeInfinity;
        MaxValue = double.PositiveInfinity;
        Inclusive = true;
    }
    
    public Range(double minValue = double.NegativeInfinity, double maxValue = double.PositiveInfinity, bool inclusive = true)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        Inclusive = inclusive;
    }
    
    public bool ContainsValue(double value)
    {
        if (Inclusive)
        {
            return value >= MinValue && value <= MaxValue;
        }
        return value > MinValue && value < MaxValue;
    }
    public bool ContainsRange(Range otherRange) => ContainsValue(otherRange.MinValue) && ContainsValue(otherRange.MaxValue);
    
    public bool IntersectsRange(Range otherRange) => ContainsValue(otherRange.MinValue) || ContainsValue(otherRange.MaxValue);

    public static Range Intersect(Range r1, Range r2) =>
        new(r1.MinValue > r2.MinValue ? r1.MinValue : r2.MinValue,
            r1.MaxValue < r2.MaxValue ? r1.MaxValue : r2.MaxValue);

    public double LerpBetween(double weight) => Mathf.Lerp(MinValue, MaxValue, weight);

    public static implicit operator string(Range range) => $"Minvalue: {range.MinValue}, Maxvalue: {range.MaxValue}";
}