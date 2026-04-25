
using System.Text.Json.Serialization;
using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public class FuelTankJSON : PartComponentJSON
{
    public FuelLevel[] FuelLevels;

    public FuelTankJSON(FuelTank tank, int index) : base(tank, index)
    {
        FuelLevels = new FuelLevel[tank.FuelTypes.Count];
        for(int i = 0; i < tank.FuelTypes.Count; i++)
        {
            var fuelType = tank.FuelTypes[i];
            FuelLevels[i] = new FuelLevel(fuelType.Name, tank.FuelLevels[fuelType], tank.MaxFuelLevels[fuelType]);
        }
    }

    [JsonConstructor]
    public FuelTankJSON(int index, double baseMass, FuelLevel[] fuelLevels) : base(index, baseMass)
    {
        FuelLevels = fuelLevels;
    }

    public override PartComponent InitializeComponent(PartComponent component)
    {
        var tank = (FuelTank)component;
        foreach (var level in FuelLevels)
        {
            var type = FuelType.FromName(level.FuelType);
            tank.FuelLevels[type] = level.Level;
            GD.Print(level.Level);
            tank.MaxFuelLevels[type] = level.Max;
        }

        return tank;
    }
    public class FuelLevel
    {
        public string FuelType;
        public double Level;
        public double Max;
    
        [JsonConstructor]
        public FuelLevel(string fuelType, double level, double max)
        {
            FuelType = fuelType;
            Level = level;
            Max = max;
        }
    }
}