using Godot;

namespace GodotPrototype.Scripts.Simulation.ReferenceFrames;

public class NestedPosition
{
	public CoordinateSpace CoordLayer;
	public Vector3 LocalPosition;
	public CelestialScript ParentCelestial;

	public NestedPosition(Vector3 localPosition, CelestialScript parentCelestial)
	{
		LocalPosition = localPosition;
		ParentCelestial = parentCelestial;
		CoordLayer = parentCelestial.CoordLayer.Increment();
	}
	public NestedPosition(NestedPosition nestedPosition)
	{
		LocalPosition = nestedPosition.LocalPosition;
		ParentCelestial = nestedPosition.ParentCelestial;
		CoordLayer = nestedPosition.CoordLayer;
	}
	public NestedPosition(Vector3 localPosition)
	{
		LocalPosition = localPosition;
		ParentCelestial = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	public NestedPosition() // Base Case
	{
		LocalPosition = new Vector3(0, 0, 0);
		ParentCelestial = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}

	public Vector3 GetPositionAtLayer(CoordinateSpace layer)
	{
		// Layer 0 is Renderspace, Layer 1 is GalaxySpace, 2 is StarSpace, 3 is PlanetSpace, 4 is MoonSpace and 5+ are levels of nested MoonSpace
		if (layer == CoordinateSpace.RenderSpace)
		{
			var convertedPosition = new Vector3();
			for (int i = 1; i < Mathf.Max((int)CoordLayer, (int)Freecam.NestedPos.CoordLayer + 1); i++)
			{
				convertedPosition += (GetPositionAtLayer((CoordinateSpace)i) - Freecam.NestedPos.GetPositionAtLayer((CoordinateSpace)i)) * ((CoordinateSpace)i).GetConversionFactor(0) * GlobalValues.Scale;
			}
			return convertedPosition;
		}
		if (layer == CoordLayer)
		{
			return LocalPosition;
		}

		if (layer < CoordLayer)
		{
			return ParentCelestial.NestedPos.GetPositionAtLayer(layer);
		}

		return new Vector3();
	}

	public static Vector3 ConvertPositionReference(NestedPosition conversionPosition, NestedPosition newRefPosition)
	{
		var convertedPosition = new Vector3();
		for (var i = 1; i < Mathf.Max((int)conversionPosition.CoordLayer, (int)newRefPosition.CoordLayer) + 1; i++)
		{
			convertedPosition +=
				(conversionPosition.GetPositionAtLayer((CoordinateSpace)i) -
				 newRefPosition.GetPositionAtLayer((CoordinateSpace)i)) *
				((CoordinateSpace)i).GetConversionFactor(newRefPosition.CoordLayer);
		}
		return convertedPosition;
	}

}
