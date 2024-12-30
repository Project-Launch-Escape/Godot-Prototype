using Godot;
using GodotPrototype.Scripts.Simulation.Physics;

namespace GodotPrototype.Scripts.UserInterface;

public partial class OrbitMesh : MeshInstance3D
{
	private Orbit _orbit;
	private Dictionary<string,Control> _labels = [];

	private const ushort OrbitVerticesCount = 2000;
	
	private static readonly PackedScene OrbitMeshPrefab = GD.Load<PackedScene>("res://Prefabs/orbit_mesh.tscn");
	private static readonly PackedScene OrbitLabelPrefab = GD.Load<PackedScene>("res://Prefabs/orbit_label.tscn");
	public static Node DefaultMeshParent;
	
	private static ImmediateMesh GenerateMesh(Orbit orbit)
	{
		var orbitMesh = new ImmediateMesh();
		var orbitMaterial = new StandardMaterial3D();
			
		orbitMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		orbitMaterial.AlbedoColor = orbit.Color;
				
		orbitMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip, orbitMaterial);
		orbitMesh.SurfaceSetColor(orbit.Color);

		var conicRange = orbit.OrbitType is ConicType.Circular or ConicType.Elliptical ?
			Math.PI : Math.Abs(Math.Acos(1 / orbit.e) - Math.PI);
		
		for (int i = 0; i <= OrbitVerticesCount; i++)
		{
			var vertexPosition = orbit.PositionFromTrueAnomaly(Mathf.Lerp(-conicRange, conicRange, (double)i / OrbitVerticesCount));
			if (vertexPosition.Magnitude() > orbit.ParentCelestial.SOIRadius) continue;
			orbitMesh.SurfaceAddVertex((Vector3)vertexPosition);
		}
		orbitMesh.SurfaceEnd();
		return orbitMesh;
	}
	
	public static OrbitMesh CreateOrbitLine(Orbit orbit)
	{
		var orbitMeshNode = OrbitMeshPrefab.Instantiate();

		if (orbit.ParentCelestial != null)
		{
			var orbitMesh = GenerateMesh(orbit);
			((MeshInstance3D)orbitMeshNode).Mesh = orbitMesh;
			orbit.ParentCelestial.AddChild(orbitMeshNode);
		}
		else
		{
			DefaultMeshParent.AddChild(orbitMeshNode);
		}
		
		var orbitMeshScript = (OrbitMesh)orbitMeshNode;
		orbitMeshScript._orbit = orbit;

		return orbitMeshScript;
	}

	public void DeleteOrbitLine()
	{
		QueueFree();
	}

	public void UpdateOrbitLine(Orbit newOrbit)
	{
		Mesh = GenerateMesh(newOrbit);
		
		if (newOrbit.ParentCelestial != GetParent())
		{ 
			Reparent(newOrbit.ParentCelestial);
			Position = Vector3.Zero;
			Scale = Vector3.One;
		}
		_orbit = newOrbit;
	}

	public void CreateOrbitLabelAtTrueAnomaly(string labelName, double trueAnomaly, string labelText)
	{
		var label = OrbitLabelPrefab.Instantiate();
		label.Name = labelName;
		((Label)label.FindChild("Label")).Text = labelText;
	}

	public override void _Process(double delta)
	{
		 if (Visible != GlobalValues.UIVisible) Visible = GlobalValues.UIVisible;
	}
}
