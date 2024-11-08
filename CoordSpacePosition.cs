using Godot;
using System;

namespace CustomTypes;

public class CoordSpacePosition
{
	public CoordinateSpace CoordLayer;
	public Vector3 LocalPosition;
	public CelestialScript ParentCelestial;

	public CoordSpacePosition(Vector3 _LocalPosition, CelestialScript _ParentCelestial)
	{
		LocalPosition = _LocalPosition;
		ParentCelestial = _ParentCelestial;
		CoordLayer = (CoordinateSpace)((int)_ParentCelestial.CoordLayer + 1);
	}
	public CoordSpacePosition(CoordSpacePosition _CoordSpacePosition)
	{
		LocalPosition = _CoordSpacePosition.LocalPosition;
		ParentCelestial = _CoordSpacePosition.ParentCelestial;
		CoordLayer = _CoordSpacePosition.CoordLayer;
	}
	public CoordSpacePosition(Vector3 _LocalPosition)
	{
		LocalPosition = _LocalPosition;
		ParentCelestial = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	public CoordSpacePosition() // Base Case
	{
		CoordLayer = CoordinateSpace.GalaxySpace;
		LocalPosition = new Vector3(0, 0, 0);
		ParentCelestial = null;
	}

	public Vector3 GetPositionAtLayer(CoordinateSpace Layer)
	{
		// Layer 0 is Renderspace, Layer 1 is GalaxySpace, 2 is StarSpace, 3 is PlanetSpace, 4 is MoonSpace and 5+ are levels of nested MoonSpace
		if (Layer == CoordinateSpace.RenderSpace)
		{
			Vector3 RenderSpacePos = new Vector3();
			for (int i = 1; i < Mathf.Max((int)CoordLayer, (int)Freecam.CoordLayer)+1; i++)
			{
				RenderSpacePos += (GetPositionAtLayer((CoordinateSpace)i) - Freecam.CoordPositions.GetPositionAtLayer((CoordinateSpace)i)) * GlobalValues.GetRefConversionFactor((CoordinateSpace)i, 0) * GlobalValues.Scale;
			}
			return RenderSpacePos;
		}
		if (Layer == CoordLayer)
		{
			return LocalPosition;
		}
		else if (Layer < CoordLayer)
		{
			return ParentCelestial.CoordPositions.GetPositionAtLayer(Layer);
		}
		else
		{
			return new Vector3();
		}
	}

	//public static CoordSpacePosition ConvertPosition(CoordSpacePosition Position1, CoordSpacePosition Position2)
	//{
	//	
	//} Unfinished Method
}
public enum CoordinateSpace
{
	RenderSpace = 0,
	GalaxySpace = 1,
	StarSpace = 2,
	PlanetSpace = 3,
	MoonSpace = 4,
};
