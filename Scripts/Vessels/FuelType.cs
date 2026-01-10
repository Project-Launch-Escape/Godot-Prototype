using Godot;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass]
public partial class FuelType : Resource
{
	[Export] public string Name;
	[Export] public double Mass; // Mass in kg of one unit of fuel
	[Export] public double Volume; // Volume in L of one unit of fuel
	[Export] public Color Color;

	public double Density => Mass / Volume; // Density in kg/L

	private const string ResourceFolderPath = "res://Resources/FuelTypes/";
	public static readonly List<FuelType> AllFuelTypes = [];

	public static void InitializeFuelTypes()
	{
		var fileNames = DirAccess.GetFilesAt(ResourceFolderPath);
		foreach (var fileName in fileNames)
		{
			var filePath = ResourceFolderPath + fileName;
			var fuelType = ResourceLoader.Load<FuelType>(filePath);
			
			AllFuelTypes.Add(fuelType);
		}
	}

	public FuelType(string stringName, double massPerUnit, double volumePerUnit)
	{
		Name = stringName;
		Mass = massPerUnit;
		Volume = volumePerUnit;
	}

	public FuelType()
	{
	}

	public static FuelType FromName(string name)
	{
		foreach (var fuelType in AllFuelTypes)
		{
			
			if (fuelType.Name.Equals(name)) return fuelType;
		}

		throw new Exception("Requested fuel type does not exist");
	}
}
