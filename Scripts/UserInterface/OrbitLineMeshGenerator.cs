using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using MeshInstance3D = Godot.MeshInstance3D;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;

namespace GodotPrototype.Scripts.UserInterface;

public partial class OrbitLineMeshGenerator : MeshInstance3D
{
	private static List<Orbit> _orbits = [];
	
	private const int OrbitVerticesCount = 5000;
	private static Node _meshParent;
	private static List<MeshInstance3D> _orbitMeshNodes = [];
	
	public override void _Ready()
	{
		_meshParent = this;
	}
	
	public static void CreateOrbitLine(Orbit orbit)
	{
		if (orbit.ParentCelestial == null) return;
		_orbits.Add(orbit);
		
		var orbitMesh = new ImmediateMesh();
		var orbitMaterial = new StandardMaterial3D();
			
		orbitMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		orbitMaterial.AlbedoColor = orbit.Color;
				
		orbitMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip, orbitMaterial);
		orbitMesh.SurfaceSetColor(orbit.Color);
		
		var coordLayer = orbit.ParentCelestial.NestedPos.CoordLayer.Increment();
		
		for (int i = 0; i < OrbitVerticesCount; i++)
		{
			var vertexPosition = orbit.GetPositionAtE(i * Math.Tau / (OrbitVerticesCount - 1));
			orbitMesh.SurfaceAddVertex((Vector3)(vertexPosition * coordLayer.GetConversionFactor(0)));
		}

		orbitMesh.SurfaceEnd();
		var orbitMeshNode = new MeshInstance3D();
		orbitMeshNode.Mesh = orbitMesh;
		
		_meshParent.AddChild(orbitMeshNode);
		_orbitMeshNodes.Add(orbitMeshNode);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		for (int i = 0; i < _orbitMeshNodes.Count; i++)
		{
			var renderSpacePos = _orbits[i].ParentCelestial.NestedPos[0];
			_orbitMeshNodes[i].Position = (Vector3)renderSpacePos.Normalized();
			_orbitMeshNodes[i].Scale = (Vector3)(Vector3d.One / renderSpacePos.Magnitude());
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.Pressed && inputEventKey.Keycode == Key.M)
			{
				Visible = !Visible;
			}
		}
	}
}
