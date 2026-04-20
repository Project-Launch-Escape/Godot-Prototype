

using Godot;

namespace GodotPrototype.Scripts.Vessels;

public class FuelSystem
{
	public List<FuelTank> ConnectedTanks = [];
	private List<FuelRequest> _fuelRequests = [];
	
	/// Represents a request to a FuelSystem to either drain or fill fuel. Requests are handled once per frame.
	/// A positive fuel delta represents a fill request, and the opposite represents a drain request. Zero requests should not be made.
	/// All FuelRequests are assumed to only contain either fill or drain requests
	private class FuelRequest
	{
		public Dictionary<FuelType, double> FuelDeltas;
		public Dictionary<FuelType, double> Fulfillment;
		public Action<Dictionary<FuelType, double>> Callback;
		public bool IsDrainRequest; 

		public FuelRequest(Dictionary<FuelType, double> fuelDeltas, Action<Dictionary<FuelType, double>> callback)
		{
			FuelDeltas = fuelDeltas;
			Callback = callback;
			Fulfillment = new Dictionary<FuelType, double>(); // TODO: Add 'FulfillmentType' enum so that zero requests can be handled
			foreach (var delta in FuelDeltas)
			{
				Fulfillment.Add(delta.Key, 0d);
				IsDrainRequest = delta.Value < 0;
			}
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
		/// Returns the actual change in fulfillment. Only change fulfillment in the direction of FuelDelta
		public double FulfillBy(FuelType fuelType, double deltaFulfillment)
		{
			if ((IsDrainRequest && Fulfillment[fuelType] + deltaFulfillment > FuelDeltas[fuelType]) ||
				!IsDrainRequest && Fulfillment[fuelType] + deltaFulfillment < FuelDeltas[fuelType])
			{
				var actualDelta = FuelDeltas[fuelType] - Fulfillment[fuelType];
			
				Fulfillment[fuelType] = FuelDeltas[fuelType];
				return actualDelta;
			}

			Fulfillment[fuelType] += deltaFulfillment;
			return deltaFulfillment;
		}
	}
	/// <summary>Requests a change in fuel (drain or fill), and calls back with the actual drain in fuel</summary>
	/// <param name="fuelDeltas">The requested changes in fuel. represented by a dictionary with FuelType keys and double values.
	/// A negative value corresponds to a drain request, and a positive value corresponds to a fill request.</param>
	/// <param name="callback">The method that will be called with the actual drain amount once fuel requests are processed</param>
	public void AddFuelRequest(Dictionary<FuelType, double> fuelDeltas, Action<Dictionary<FuelType, double>> callback)
	{
		_fuelRequests.Add(new FuelRequest(fuelDeltas, callback));
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
				if (request.IsDrainRequest) drainRequests.Add(request);
				else fillRequests.Add(request);
			}
			if (Engine.GetFramesDrawn() % 24 == 0)GD.PrintT($"fill requests: {fillRequests.Count}, drain requests:{drainRequests.Count}");

			foreach (var drainRequest in drainRequests)
			{
				// Request fulfillments from drain requests evenly, removing fulfilled requests.
				// When requests are fulfilled, keep doing passes until either all fill requests are fulfilled or drain request is fulfilled
				while (!drainRequest.IsFuelTypeFulfilled(fuelType) && fillRequests.Count > 0)
				{
					double drainAmount = (drainRequest.Fulfillment[fuelType] - drainRequest.FuelDeltas[fuelType]) / drainRequests.Count;
					for (var i = 0; i < fillRequests.Count; i++)
					{
						var fillRequest = fillRequests[i];
						var actualDrain = fillRequest.FulfillBy(fuelType, drainAmount);
						drainRequest.FulfillBy(fuelType, -actualDrain);
						if (actualDrain != drainAmount)
						{
							fillRequests.RemoveAt(i);
							break;
						}
					}
				}
				if (drainRequest.IsFuelTypeFulfilled(fuelType)) continue;
				drainRequest.FulfillBy(fuelType, ChangeFuelAmount(fuelType, drainRequest.FuelDeltas[fuelType] - drainRequest.Fulfillment[fuelType]));
			}

			foreach (var fillRequest in fillRequests)
			{
				fillRequest.FulfillBy(fuelType, ChangeFuelAmount(fuelType, fillRequest.Fulfillment[fuelType] - fillRequest.FuelDeltas[fuelType]));
			}
		}
		
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
			if (tank.HasFuelType(fuelType)) usableTanks.Add(tank);
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
