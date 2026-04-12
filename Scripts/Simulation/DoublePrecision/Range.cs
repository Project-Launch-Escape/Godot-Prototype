using Godot;

namespace GodotPrototype.Scripts.Simulation.DoublePrecision;

public readonly struct Range
{
    public readonly double MinValue;
    public readonly double MaxValue;
    
    public readonly bool Inclusive;

    public bool IsInfinite => double.IsInfinity(MinValue) || double.IsInfinity(MaxValue);
    public double Width => MaxValue - MinValue;
    
    /// A Range is valid if its Max Value is greater than or equal to its Min Value
    public bool IsValid => MinValue <= MaxValue;

    /// Represents a range covering all real numbers
    public static readonly Range Unbounded = new();
    /// Represents a range covering all positive numbers (including zero)
    public static readonly Range ToPositiveInfinity = new(0);
    /// Represents a range covering all negative numbers (including zero)
    public static readonly Range ToNegativeInfinity = new(maxValue:0);
    /// Represents a range spanning the unit circle from [-Pi, Pi]
    public static readonly Range FullCirle = new(-Math.PI, Math.PI);

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

    public static implicit operator string(Range range) => $"Minvalue: {range.MinValue:F3}, Maxvalue: {range.MaxValue:F3}";
}