using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.Simulation;

public partial class LocalSurface : MeshInstance3D, IRenderable
{
	private static Celestial CurrentSOI => Vessel.ActiveVessel.ParentBody;
	private static MeshInstance3D _meshObject;

	public override void _Ready()
	{
		FlightCamera.AddToRenderSpaceUpdate(this);
		_meshObject = this;
	}

	public static void OnSOIChange(Celestial newCelestial)
	{
		if (newCelestial == null) return;
		if (newCelestial.SurfaceNode == null)
		{
			_meshObject.Visible = false;
			return;
		}

		_meshObject.Visible = true;
		_meshObject.Mesh = newCelestial.SurfaceNode.Mesh;
		_meshObject.MaterialOverride = newCelestial.SurfaceNode.MaterialOverride;
		_meshObject.Scale = Vector3.One * 2 * (float)newCelestial.Radius;
	}

	public void RenderUpdate()
	{
		Position = (Vector3)CurrentSOI.RelPosition[CoordinateSpace.VesselSpace];
	}
}
