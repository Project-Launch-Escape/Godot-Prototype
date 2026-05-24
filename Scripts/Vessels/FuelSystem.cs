

using Godot;

namespace GodotPrototype.Scripts.Vessels;

public class FuelSystem
{
	public List<FuelTank> ConnectedTanks = [];
	private List<FuelRequest> _fuelRequests = [];

	/// <summary>Requests a change in fuel (drain or fill), and calls back with the actual drain in fuel</summary>
	/// <param name="fuelDeltas">The requested changes in fuel. represented by a dictionary with FuelType keys and double values.
	/// A negative value corresponds to a drain request, and a positive value corresponds to a fill request.</param>
	/// <param name="callback">The method that will be called with the actual drain amount once fuel requests are processed</param>
	public void AddFuelRequest(Dictionary<FuelType, double> fuelDeltas, Action<Dictionary<FuelType, double>> callback)
	{
		_fuelRequests.Add(new FuelRequest(fuelDeltas, callback));
	}

	public void PrintRequests()
	{
		string str = "";
		foreach (var fuelRequest in _fuelRequests)
		{
			str += fuelRequest + "\n";
		}

		GD.Print(str);
	}

	public void HandleFuelRequests() //TODO: Make this shithole even more complicated by only making requests drain in exact ratios
	{
		// Drain requests first attempt to drain from fill requests, then from tanks
		// Then any remaining fill requests attempt to fill into tanks
		var requestDict = new Dictionary<FuelType, List<FuelRequest>>();
		foreach (var fuelRequest in _fuelRequests)
		{
			foreach (var delta in fuelRequest.FuelDeltas)
			{
				if (requestDict.TryGetValue(delta.Key, out var requestList))
				{
					requestList.Add(fuelRequest);
				}
				else
				{
					requestDict.Add(delta.Key, [fuelRequest]);
				}
			}
		}

		foreach (var fuelType in requestDict.Keys)
		{
			var requests = requestDict[fuelType];
			var fillRequests = new List<FuelRequest>();
			var drainRequests = new List<FuelRequest>();
			foreach (var request in requests)
			{
				switch (request.RequestType)
				{
					case FuelRequestType.Drain:
						drainRequests.Add(request);
						break;
					case FuelRequestType.Fill:
						fillRequests.Add(request);
						break;
				}
			}

			foreach (var drainRequest in drainRequests)
			{
				// Request fulfillments from drain requests evenly, removing fulfilled requests.
				// When requests are fulfilled, keep doing passes until either all fill requests are fulfilled or drain request is fulfilled
				var fillRequests2 = fillRequests.ToList();
				while (!drainRequest.IsFuelTypeFulfilled(fuelType) && fillRequests2.Count > 0)
				{
					double drainAmount = (drainRequest.Fulfillment[fuelType] - drainRequest.FuelDeltas[fuelType]) / drainRequests.Count;
					for (var i = 0; i < fillRequests2.Count; i++)
					{
						var fillRequest = fillRequests2[i];
						var actualDrain = fillRequest.FulfillBy(fuelType, drainAmount);
						drainRequest.FulfillBy(fuelType, -actualDrain);
						if (actualDrain != drainAmount)
						{
							fillRequests2.RemoveAt(i);
							break;
						}
					}
				}
				if (drainRequest.IsFuelTypeFulfilled(fuelType)) continue;
				var df = ChangeFuelAmount(fuelType, drainRequest.FuelDeltas[fuelType] - drainRequest.Fulfillment[fuelType]);
				drainRequest.FulfillBy(fuelType, df);
				//GD.Print($"Fulfilling {fuelType.Name} by {df:F5}U (New = {drainRequest.Fulfillment[fuelType]:F4})");
			}

			foreach (var fillRequest in fillRequests)
			{
				fillRequest.FulfillBy(fuelType, ChangeFuelAmount(fuelType, fillRequest.FuelDeltas[fuelType] - fillRequest.Fulfillment[fuelType]));
			}
		}
		//if (Engine.GetFramesDrawn() % 100 == 0) PrintRequests();
		for (var i = _fuelRequests.Count - 1; i >= 0; i--)
		{
			_fuelRequests[i].SendCallBack();
			_fuelRequests.RemoveAt(i);
		}
	}
	/// Returns the actual change in fuel
	private double ChangeFuelAmount(FuelType fuelType, double deltaFuel)
	{
		// TODO: Cache tanks that have certain fuel types
		var usableTanks = new List<FuelTank>(); // Tanks in system that contain the desired fuel type
		foreach (var tank in ConnectedTanks)
		{
			if (tank.HasFuelType(fuelType) && tank.FuelLevels[fuelType] > 0) usableTanks.Add(tank);
		}

		if (usableTanks.Count == 0) return 0;

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
