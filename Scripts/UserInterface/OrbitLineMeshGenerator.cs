using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using MeshInstance3D = Godot.MeshInstance3D;

namespace GodotPrototype.Scripts.UserInterface;
public partial class OrbitLineMeshGenerator : MeshInstance3D
{
	private List<CelestialScript> _celestials = [];
	
	[Export] private int _orbitVerticesCount;
	private List<NestedPosition[]> _orbitVertices = [];

	[Export] public Camera3D CameraPosition;
	private List<MeshInstance3D> _orbitMeshNodes = [];
	private bool _calculatedPositions;

	private void InitializeMeshes()
	{
		foreach (var celestial in GlobalValues.AllCelestials)
		{
			if (celestial.ParentCelestial == null) continue;
			_celestials.Add(celestial);
		}

		foreach (var celestial in _celestials)
		{
			var orbitVertexArray = new NestedPosition[_orbitVerticesCount];
			for (int i = 0; i < _orbitVerticesCount; i++)
			{
				orbitVertexArray[i] = new NestedPosition(celestial.GetPositionAtE(i*(float)Math.Tau/(_orbitVerticesCount-1)), celestial.ParentCelestial);
			}
			_orbitVertices.Add(orbitVertexArray);
		}
		
	}

	private void CreateOrbitMeshNodes()
	{
		for (int i = 0; i < _celestials.Count; i++)
		{
			var orbitMesh = new ImmediateMesh();
			var orbitMaterial = new StandardMaterial3D();
			
			orbitMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
			orbitMaterial.AlbedoColor = _celestials[i].CelestialColor;
				
			orbitMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip, orbitMaterial);
			orbitMesh.SurfaceSetColor(_celestials[i].CelestialColor);
			
			for (int j = 0; j < _orbitVerticesCount; j++)
			{
				var coordLayer = _orbitVertices[i][j].ParentCelestial.CoordLayer.Increment();
				orbitMesh.SurfaceAddVertex(_orbitVertices[i][j].GetPositionAtLayer(coordLayer) * GlobalValues.Scale * coordLayer.GetConversionFactor(0));
			}

			orbitMesh.SurfaceEnd();
			var orbitMeshNode = new MeshInstance3D();
			orbitMeshNode.Mesh = orbitMesh;
			
			AddChild(orbitMeshNode);
			_orbitMeshNodes.Add(orbitMeshNode);
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!_calculatedPositions)
		{
			_calculatedPositions = true;
			InitializeMeshes();
			CreateOrbitMeshNodes();
		}
		
		for (int i = 0; i < _orbitMeshNodes.Count; i++)
		{
			_orbitMeshNodes[i].Position = _celestials[i].ParentCelestial.Position;
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
