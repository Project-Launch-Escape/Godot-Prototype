using Godot;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/FuelTankIcon.png")]
public partial class FuelTank : PartComponent
{
    [Export] public Godot.Collections.Dictionary<FuelType, double> FuelLevels = [];
    [Export] public Godot.Collections.Dictionary<FuelType, double> MaxFuelLevels = [];

    public List<FuelType> FuelTypes => FuelLevels.Keys.ToList();
    
    /// <summary>Changes the amount of a specific fuel type in the tank by a specified amount (in units)</summary>
    /// <param name="deltaFuel">Change in fuel requested, negative for drain, positive for fill</param>
    /// <param name="fuelType">String name of the fuel type requested to change</param>
    /// <returns>Amount of fuel sent back due to requested fuel change exceeding minimum or maximum.
    /// Positive sendback indicates overfill, negative sendback indicates excess drain. deltaFuel - sendback = actual change in fuel</returns>
    
    public double ChangeFuelAmount(double deltaFuel, FuelType fuelType)
    {
        if (FuelLevels[fuelType] + deltaFuel < 0)
        {
            var sendBack = deltaFuel + FuelLevels[fuelType];
            
            FuelLevels[fuelType] = 0;
            return sendBack;
            // Requested fuel drain exceeds fuel available; returns amount of excess in request (negative)
        }
        if (FuelLevels[fuelType] + deltaFuel > MaxFuelLevels[fuelType])
        {
            var sendBack = deltaFuel - (MaxFuelLevels[fuelType] - FuelLevels[fuelType]);
            
            FuelLevels[fuelType] = MaxFuelLevels[fuelType];
            return sendBack;
            // Requested fill would overfill tank, returns amount of excess in fill (positive)
        }

        FuelLevels[fuelType] += deltaFuel;
        return 0;
    }

    public void SetFuelAmount(double newFuel, FuelType fuelType)
    {
        FuelLevels[fuelType] = newFuel;
    }

    protected override double GetMass() => BaseMass + FuelLevels.Sum(fuelLevel => fuelLevel.Value * fuelLevel.Key.Mass);

    public bool HasFuelType(FuelType fuelType) => FuelLevels.ContainsKey(fuelType) && FuelLevels[fuelType] > 0;
}