using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.Simulation.ReferenceFrames;

public class RelativeVelocity
{
	public CoordinateSpace CoordLayer { get; private set; }
	public Vector3d LocalVelocity;
	private RelativeVelocity _refVelocity;
	public RelativeVelocity ReferenceVelocity
	{
		get => _refVelocity;
		set
		{
			_refVelocity = value;
			CoordLayer = value == null ? CoordinateSpace.GalaxySpace : value.CoordLayer + 1;
		}
	}

	public RelativeVelocity(Vector3d localVelocity, Celestial parentCelestial)
	{
		LocalVelocity = localVelocity ?? new Vector3d();
		if (parentCelestial != null)
		{
			ReferenceVelocity = parentCelestial.VelocityRel;
			return;
		}

		ReferenceVelocity = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	
	public RelativeVelocity(Vector3d localVelocity, RelativeVelocity referenceVelocity)
	{
		LocalVelocity = localVelocity;
		ReferenceVelocity = referenceVelocity;
	}
	
	public RelativeVelocity(RelativeVelocity relativeVelocity)
	{
		LocalVelocity = relativeVelocity.LocalVelocity;
		ReferenceVelocity = relativeVelocity.ReferenceVelocity;
	}
	
	public RelativeVelocity(Vector3d localPosition)
	{
		LocalVelocity = localPosition;
		ReferenceVelocity = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}
	
	public RelativeVelocity() // Base Case
	{
		LocalVelocity = new Vector3d();
		ReferenceVelocity = null;
		CoordLayer = CoordinateSpace.GalaxySpace;
	}

	public Vector3d this[CoordinateSpace coordLayer] => GetVelocityAtLayer(coordLayer);

	private Vector3d GetVelocityAtLayer(CoordinateSpace layer)
	{
		// Layer 0 is GalaxySpace, 1 is StarSpace, 2 is PlanetSpace, 3 is MoonSpace and 4+ are levels of nested MoonSpace

		if (layer == CoordLayer) return LocalVelocity;
		
		if (layer < CoordLayer && ReferenceVelocity != null) return ReferenceVelocity.GetVelocityAtLayer(layer);

		return CoordLayer switch
		{
			CoordinateSpace.RenderSpace => throw new NotSupportedException(),
			CoordinateSpace.AbsoluteSpace => GetAbsoluteVelocity(),
			CoordinateSpace.VesselSpace => VelocityRefConverted(Vessel.ActiveVessel.VelocityRel),
			_ => new Vector3d()
		};
	}
	
	private Vector3d GetAbsoluteVelocity()
	{
		var absolutepos = new Vector3d();
		for (CoordinateSpace layer = 0; layer < CoordLayer; layer++)
		{
			absolutepos += GetVelocityAtLayer(layer);
		}

		return absolutepos;
	}
	
	public void ConvertRef(RelativeVelocity newRef)
	{
		var newRefVelocity = new RelativeVelocity();
		if (newRef != null)
		{
			newRefVelocity = newRef;
		}
		
		LocalVelocity = VelocityRefConverted(newRefVelocity);
		ReferenceVelocity = newRef;
	}

	private Vector3d VelocityRefConverted(RelativeVelocity newRefVelocity)
	{
		var convertedVelocity = new Vector3d();
		for (CoordinateSpace layer = 0; layer < (CoordinateSpace)Math.Max((int)CoordLayer, (int)newRefVelocity.CoordLayer) + 1; layer++)
		{
			convertedVelocity += this[layer] - newRefVelocity[layer];
		}
		return convertedVelocity;
	}
	
	public static implicit operator string(RelativeVelocity nestedPos)
	{
		return $"LocalPosition: ({(string)nestedPos.LocalVelocity}), CoordLayer: {nestedPos.CoordLayer}";
	}
}
