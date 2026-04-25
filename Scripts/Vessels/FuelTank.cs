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
    /// <param name="fuelType">fuel type requested to change</param>
    /// <returns>Returns the actual change in fuel. Equal to deltaFuel if tank is not under/over filled</returns>
    
    public double ChangeFuelAmount(double deltaFuel, FuelType fuelType)
    {
        if (FuelLevels[fuelType] + deltaFuel < 0)
        {
            var actualDelta = -FuelLevels[fuelType];
            
            FuelLevels[fuelType] = 0;
            return actualDelta;
            // Requested fuel drain exceeds fuel available; returns amount stored in tank (negative)
        }
        if (FuelLevels[fuelType] + deltaFuel > MaxFuelLevels[fuelType])
        {
            var actualDelta = MaxFuelLevels[fuelType] - FuelLevels[fuelType];
            
            FuelLevels[fuelType] = MaxFuelLevels[fuelType];
            return actualDelta;
            // Requested fill would overfill tank, returns difference between tank level and tank max (positive)
        }

        FuelLevels[fuelType] += deltaFuel;
        return deltaFuel;
    }

    public void SetFuelAmount(double newFuel, FuelType fuelType)
    {
        FuelLevels[fuelType] = newFuel;
    }

    protected override double GetMass() => BaseMass + FuelLevels.Sum(fuelLevel => fuelLevel.Value * fuelLevel.Key.Mass);

    public bool HasFuelType(FuelType fuelType) => FuelLevels.ContainsKey(fuelType);
}