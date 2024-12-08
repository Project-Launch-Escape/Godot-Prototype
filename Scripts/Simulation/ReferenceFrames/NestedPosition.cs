using Godot;
using GodotPrototype.Scripts.Simulation.DoublePrecision;

namespace GodotPrototype.Scripts.Simulation.ReferenceFrames;

public class NestedPosition
{
	public CoordinateSpace CoordLayer;
	public Vector3d LocalPosition;
	public NestedPosition ParentPosition;

	public NestedPosition(Vector3d localPosition, CelestialScript parentCelestial)
	{
		LocalPosition = localPosition;
		ParentPosition = parentCelestial.NestedPos;
		CoordLayer = parentCelestial.NestedPos.CoordLayer.Increment();
	}
	
	public NestedPosition(Vector3d localPosition, NestedPosition parentPosition)
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
	
	public NestedPosition(Vector3d localPosition)
	{
		LocalPosition = localPosition;
		ParentPosition = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	
	public NestedPosition() // Base Case
	{
		LocalPosition = new Vector3d();
		ParentPosition = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}

	public Vector3d this[CoordinateSpace coordLayer]
	{
		get
		{
			if (coordLayer < 0) throw new IndexOutOfRangeException();
			return GetPositionAtLayer(coordLayer);
		}
	} 

	public Vector3d GetPositionAtLayer(CoordinateSpace layer)
	{
		// Layer 0 is RenderSpace, Layer 1 is GalaxySpace, 2 is StarSpace, 3 is PlanetSpace, 4 is MoonSpace and 5+ are levels of nested MoonSpace
		if (layer == CoordinateSpace.RenderSpace)
		{
			return ConvertPositionReference(Freecam.NestedPos);
		}
		if (layer == CoordLayer) return LocalPosition;
		
		if (layer < CoordLayer && ParentPosition != null) return ParentPosition.GetPositionAtLayer(layer);

		return new Vector3d();
	}

	public Vector3d ConvertPositionReference(NestedPosition newRefPosition)
	{
		var convertedPosition = new Vector3d();
		for (var i = 1; i < Math.Max((int)CoordLayer, (int)newRefPosition.CoordLayer) + 1; i++)
		{
			var coordLayer = (CoordinateSpace)i;
			convertedPosition += (this[coordLayer] - newRefPosition[coordLayer]);
		}
		return convertedPosition;
	}
	
	public static implicit operator string(NestedPosition nestedPos)
	{
		return $"LocalPosition: ({(string)nestedPos.LocalPosition}), CoordLayer: {nestedPos.CoordLayer}";
	}
}
