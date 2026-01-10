namespace GodotPrototype.Scripts.Vessels;

public class FuelSystem
{
    public List<FuelTank> ConnectedTanks = [];
    //public static FuelSystem BaseSystem;
    
    public double ChangeFuelAmount(double deltaFuel, FuelType fuelType)
    {
        var usableTanks = new List<FuelTank>(); // Tanks in system that contain the desired fuel type
        foreach (var tank in ConnectedTanks)
        {
            if (tank.HasFuelType(fuelType)) usableTanks.Add(tank);
        }

        if (usableTanks.Count == 0) return deltaFuel;

        var fuelSendback = 0d;
        foreach (var tank in usableTanks)
        {
            fuelSendback += tank.ChangeFuelAmount(deltaFuel / usableTanks.Count, fuelType);
        }

        return fuelSendback;
    }

    public bool HasFuelType(FuelType fuelType)
    {
        foreach (var tank in ConnectedTanks)
        {
            if (tank.HasFuelType(fuelType)) return true;
        }
        return false;
    }

    public List<FuelType> GetContainedFuelTypes()
    {
        var fuelTypes = new List<FuelType>();
        foreach (var tank in ConnectedTanks)
        {
            foreach (var fuelType in tank.FuelLevels.Keys)
            {
                if (!fuelTypes.Contains(fuelType)) fuelTypes.Add(fuelType);
            }
        }
        return fuelTypes;
    }

    public List<FuelTank> GetTanksWithFuelType(FuelType fuelType)
    {
        var tanks = new List<FuelTank>();
        foreach (var tank in ConnectedTanks)
        {
            if (tank.HasFuelType(fuelType)) tanks.Add(tank);
        }
        return tanks;
    }

    public void AddTank(FuelTank tank) => ConnectedTanks.Add(tank);

    public double GetMaxOfFuelType(FuelType fuelType)
    {
        var maxFuel = 0d;
        foreach (var tank in GetTanksWithFuelType(fuelType))
        {
            maxFuel += tank.MaxFuelLevels[fuelType];
        }
        return maxFuel;
    }
    public double GetLevelOfFuelType(FuelType fuelType)
    {
        var fuelLevel = 0d;
        foreach (var tank in GetTanksWithFuelType(fuelType))
        {
            fuelLevel += tank.FuelLevels[fuelType];
        }
        return fuelLevel;
    }
}