using Godot;

namespace GodotPrototype.Scripts.Vessels;

/// Represents a request to a FuelSystem to either drain or fill fuel. Requests are handled once per frame.
/// A positive fuel delta represents a fill request, and the opposite represents a drain request. Zero requests should not be made.
/// All FuelRequests are assumed to only contain either fill or drain requests
public class FuelRequest
{
    public Dictionary<FuelType, double> FuelDeltas;
    public Dictionary<FuelType, double> Fulfillment;
    public Action<Dictionary<FuelType, double>> Callback;
    public FuelRequestType RequestType;

    public FuelRequest(Dictionary<FuelType, double> fuelDeltas, Action<Dictionary<FuelType, double>> callback)
    {
        FuelDeltas = fuelDeltas;
        Callback = callback;
        Fulfillment = new Dictionary<FuelType, double>();
        foreach (var delta in FuelDeltas)
        {
            Fulfillment.Add(delta.Key, 0d);
        }
        RequestType = GetRequestType();
    }

    private FuelRequestType GetRequestType()
    {
        var requestType = FuelRequestType.Unassigned;
        foreach (var (_, fuelDelta) in FuelDeltas)
        {
            var newType = fuelDelta > 0 ? FuelRequestType.Fill : fuelDelta < 0 ? FuelRequestType.Drain : FuelRequestType.Zero;
            if (requestType == FuelRequestType.Unassigned)
                requestType = newType;
            else if (newType != requestType)
                throw new Exception($"Request contains multiple types! This is not supported and your execution date is 8/10/2026 at {Math.Abs((int)fuelDelta*1000340%12)}:00 pm");
        }

        return requestType;
    }

    public void SendCallBack()
    {
        Callback?.Invoke(Fulfillment);
    }

    public bool IsFuelTypeFulfilled(FuelType fuelType)
    {
        var fuelDelta = FuelDeltas[fuelType];
        return fuelDelta switch
        {
            < 0 => Fulfillment[fuelType] <= fuelDelta,
            > 0 => Fulfillment[fuelType] >= fuelDelta,
            _ => true
        };
    }
    public bool IsFulfilled()
    {
        foreach (var (fuelType, _) in FuelDeltas)
        {
            if (!IsFuelTypeFulfilled(fuelType)) return false;
        }
        return true;
    }
    
    /// Returns the actual change in fulfillment. Only change fulfillment in the direction of FuelDelta
    public double FulfillBy(FuelType fuelType, double deltaFulfillment)
    {
        bool wouldOverFulfill = RequestType switch
        {
            FuelRequestType.Drain => Fulfillment[fuelType] + deltaFulfillment < FuelDeltas[fuelType],
            FuelRequestType.Fill => Fulfillment[fuelType] + deltaFulfillment > FuelDeltas[fuelType],
            FuelRequestType.Zero => false,
            _ => throw new Exception("Request has not been initialized properly!")
        };
        if (wouldOverFulfill)
        {
            var actualDelta = FuelDeltas[fuelType] - Fulfillment[fuelType];
		
            Fulfillment[fuelType] = FuelDeltas[fuelType];
            return actualDelta;
        }

        Fulfillment[fuelType] += deltaFulfillment;
        return deltaFulfillment;
    }

    public override string ToString()
    {
        string requestTypeName = RequestType switch
        {
            FuelRequestType.Fill => "Fill",
            FuelRequestType.Drain => "Drain",
            FuelRequestType.Zero => "Zero",
            _ => "Unassigned"
        };
        string requestValueString = "";
        foreach (var (fuelType, fuelDelta) in FuelDeltas)
        {
            requestValueString += $"{fuelDelta * fuelType.Mass:F5}kg of {fuelType.Name}, ";
        }
        
        return $"{(IsFulfilled() ? "F" : "Unf")}ulfilled {requestTypeName} Request for {requestValueString}";
    }
}

public enum FuelRequestType
{
    Fill,
    Drain,
    Zero,
    Unassigned
}