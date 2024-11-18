using Godot;

namespace GodotPrototype.Scripts.Simulation.ReferenceFrames;

public class NestedPosition
{
	public CoordinateSpace CoordLayer;
	public Vector3 LocalPosition;
	public NestedPosition ParentPosition;

	public NestedPosition(Vector3 localPosition, CelestialScript parentCelestial)
	{
		LocalPosition = localPosition;
		ParentPosition = parentCelestial.NestedPos;
		CoordLayer = parentCelestial.CoordLayer.Increment();
	}
	
	public NestedPosition(Vector3 localPosition, NestedPosition parentPosition)
	{
		LocalPosition = localPosition;
		ParentPosition = parentPosition;
		CoordLayer = parentPosition.CoordLayer.Increment();
	}
	
	public NestedPosition(NestedPosition nestedPosition)
	{
		LocalPosition = nestedPosition.LocalPosition;
		ParentPosition = nestedPosition.ParentPosition;
		CoordLayer = nestedPosition.CoordLayer;
	}
	
	public NestedPosition(Vector3 localPosition)
	{
		LocalPosition = localPosition;
		ParentPosition = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	
	public NestedPosition() // Base Case
	{
		LocalPosition = new Vector3();
		ParentPosition = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}

	public Vector3 GetPositionAtLayer(CoordinateSpace layer)
	{
		// Layer 0 is RenderSpace, Layer 1 is GalaxySpace, 2 is StarSpace, 3 is PlanetSpace, 4 is MoonSpace and 5+ are levels of nested MoonSpace
		if (layer == CoordinateSpace.RenderSpace)
		{
			return ConvertPositionReference(this, Freecam.NestedPos, CoordinateSpace.RenderSpace);
		}
		if (layer == CoordLayer) return LocalPosition;
		
		if (layer < CoordLayer && ParentPosition != null) return ParentPosition.GetPositionAtLayer(layer);

		return new Vector3();
	}

	public static Vector3 ConvertPositionReference(NestedPosition conversionPosition, NestedPosition newRefPosition, CoordinateSpace newCoordLayer)
	{
		var convertedPosition = new Vector3();
		for (var i = 1; i < Mathf.Max((int)conversionPosition.CoordLayer, (int)newRefPosition.CoordLayer) + 1; i++)
		{
			convertedPosition +=
				(conversionPosition.GetPositionAtLayer((CoordinateSpace)i) -
				 newRefPosition.GetPositionAtLayer((CoordinateSpace)i)) *
				((CoordinateSpace)i).GetConversionFactor(newCoordLayer);
		}
		return convertedPosition;
	}

}
