using Godot;
using System;

namespace CustomTypes;

public class NestedPosition
{
	public CoordinateSpace CoordLayer;
	public Vector3 LocalPosition;
	public CelestialScript ParentCelestial;

	public NestedPosition(Vector3 _LocalPosition, CelestialScript _ParentCelestial)
	{
		LocalPosition = _LocalPosition;
		ParentCelestial = _ParentCelestial;
		CoordLayer = _ParentCelestial.CoordLayer.Increment();
	}
	public NestedPosition(NestedPosition _NestedPosition)
	{
		LocalPosition = _NestedPosition.LocalPosition;
		ParentCelestial = _NestedPosition.ParentCelestial;
		CoordLayer = _NestedPosition.CoordLayer;
	}
	public NestedPosition(Vector3 _LocalPosition)
	{
		LocalPosition = _LocalPosition;
		ParentCelestial = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	public NestedPosition() // Base Case
	{
		LocalPosition = new Vector3(0, 0, 0);
		ParentCelestial = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}

	public Vector3 GetPositionAtLayer(CoordinateSpace Layer)
	{
		// Layer 0 is Renderspace, Layer 1 is GalaxySpace, 2 is StarSpace, 3 is PlanetSpace, 4 is MoonSpace and 5+ are levels of nested MoonSpace
		if (Layer == CoordinateSpace.RenderSpace)
		{
			Vector3 ConvertedPosition = new Vector3();
			for (int i = 1; i < Mathf.Max((int)CoordLayer, (int)Freecam.NestedPos.CoordLayer + 1); i++)
			{
				ConvertedPosition += (GetPositionAtLayer((CoordinateSpace)i) - Freecam.NestedPos.GetPositionAtLayer((CoordinateSpace)i)) * GlobalValues.GetRefConversionFactor((CoordinateSpace)i, 0) * GlobalValues.Scale;
			}
			return ConvertedPosition;
		}
		if (Layer == CoordLayer)
		{
			return LocalPosition;
		}
		else if (Layer < CoordLayer)
		{
			return ParentCelestial.NestedPos.GetPositionAtLayer(Layer);
		}
		else
		{
			return new Vector3();
		}
	}

	public static Vector3 ConvertPositionReference(NestedPosition ConversionPosition, NestedPosition NewRefPosition)
	{
		Vector3 ConvertedPosition = new Vector3();
		for (int i = 1; i < Mathf.Max((int)ConversionPosition.CoordLayer, (int)NewRefPosition.CoordLayer) + 1; i++)
		{
			ConvertedPosition += (ConversionPosition.GetPositionAtLayer((CoordinateSpace)i) - NewRefPosition.GetPositionAtLayer((CoordinateSpace)i)) * GlobalValues.GetRefConversionFactor((CoordinateSpace)i, NewRefPosition.CoordLayer);
		}
		return ConvertedPosition;
	}

}
public enum CoordinateSpace
{
	RenderSpace = 0,
	GalaxySpace = 1,
	StarSpace = 2,
	PlanetSpace = 3,
	MoonSpace = 4,
};
public static class Extensions
{
	public static CoordinateSpace Increment(this CoordinateSpace CoordLayer)
	{
		return (CoordinateSpace)((int)CoordLayer + 1);
	}
}
