using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.Simulation.ReferenceFrames;

public class RelativePosition
{
	public CoordinateSpace CoordLayer { get; private set; }
	public Vector3d LocalPosition;
	private RelativePosition _parentPos;
	public RelativePosition ParentPosition
	{
		get => _parentPos;
		set
		{
			_parentPos = value;
			CoordLayer = value == null ? CoordinateSpace.GalaxySpace : value.CoordLayer + 1;
		}
	}

	public RelativePosition(Vector3d localPosition, Celestial parentCelestial)
	{
		LocalPosition = localPosition;
		if (parentCelestial != null)
		{
			ParentPosition = parentCelestial.PositionRel;
			return;
		}

		ParentPosition = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	
	public RelativePosition(Vector3d localPosition, RelativePosition parentPosition)
	{
		LocalPosition = localPosition;
		ParentPosition = parentPosition;
	}
	
	public RelativePosition(RelativePosition nestedPosition)
	{
		LocalPosition = nestedPosition.LocalPosition;
		ParentPosition = nestedPosition.ParentPosition;
		CoordLayer = nestedPosition.CoordLayer;
	}
	
	public RelativePosition(Vector3d localPosition)
	{
		LocalPosition = localPosition;
		ParentPosition = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	
	public RelativePosition() // Base Case
	{
		LocalPosition = new Vector3d();
		ParentPosition = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}

	public Vector3d this[CoordinateSpace coordLayer] => GetPositionAtLayer(coordLayer);

	private Vector3d GetPositionAtLayer(CoordinateSpace layer)
	{
		// Layer 0 is GalaxySpace, 1 is StarSpace, 2 is PlanetSpace, 3 is MoonSpace and 4+ are levels of nested MoonSpace
		if (layer == CoordLayer) return LocalPosition;
		
		if (layer < CoordLayer && ParentPosition != null) return ParentPosition.GetPositionAtLayer(layer);
		
		return layer switch
		{
			CoordinateSpace.RenderSpace => PositionRefConverted(FlightCamera.PositionRel),
			CoordinateSpace.AbsoluteSpace => GetAbsolutePosition(),
			CoordinateSpace.VesselSpace => PositionRefConverted(GlobalValues.FOPosition),
			_ => new Vector3d()
		};
	}

	private Vector3d GetAbsolutePosition()
	{
		var absolutePos = new Vector3d();
		for (CoordinateSpace layer = 0; layer < CoordLayer; layer++)
		{
			absolutePos += GetPositionAtLayer(layer);
		}

		return absolutePos;
	}
	
	public Celestial FindHighestSOI()
	{
		var celestials = Celestial.AllCelestials;
		var currentSOIs = new List<Celestial>();
		
		var celestialDists = new List<double>();
		
		foreach (var celestial in Celestial.AllCelestials)
		{
			celestialDists.Add(celestial.PositionRel.PositionRefConverted(this).Magnitude);
		}

		for (int i = 0; i < celestialDists.Count; i++)
		{
			if (celestialDists[i] <= celestials[i].SOIRadius) currentSOIs.Add(celestials[i]);
		}
		
		if (currentSOIs.Count == 0) return null;
		
		var highestSOILayer = CoordinateSpace.GalaxySpace;
		var highestSOIIndex = 0;
		
		for (var i = 0; i < currentSOIs.Count; i++)
		{
			if (currentSOIs[i].PositionRel.CoordLayer <= highestSOILayer) continue;
			
			highestSOILayer = currentSOIs[i].PositionRel.CoordLayer;
			highestSOIIndex = i;
		}
		
		return currentSOIs[highestSOIIndex];
	}

	public void ConvertRef(RelativePosition newRef)
	{
		var newRefPosition = new RelativePosition();
		if (newRef != null)
		{
			newRefPosition = newRef;
		}
		
		LocalPosition = PositionRefConverted(newRefPosition);
		ParentPosition = newRef;
	}

	public Vector3d PositionRefConverted(RelativePosition newRefPosition)
	{
		var convertedPosition = new Vector3d();
		for (CoordinateSpace layer = 0; layer < (CoordinateSpace)Math.Max((int)CoordLayer, (int)newRefPosition.CoordLayer) + 1; layer++)
		{
			convertedPosition += this[layer] - newRefPosition[layer];
		}
		return convertedPosition;
	}
	
	public static implicit operator string(RelativePosition nestedPos)
	{
		return $"LocalPosition: ({(string)nestedPos.LocalPosition}), CoordLayer: {nestedPos.CoordLayer}";
	}
}
