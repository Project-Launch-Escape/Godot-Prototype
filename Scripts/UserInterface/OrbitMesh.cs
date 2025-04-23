using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface;

public partial class OrbitMesh : MeshInstance3D
{
	private Orbit _orbit;
	private Dictionary<string,OrbitMarker> _markers = [];

	private const ushort OrbitVerticesCount = 512;
	
	private static readonly PackedScene OrbitMeshPrefab = GD.Load<PackedScene>("res://Prefabs/orbit_mesh.tscn");
	public static Node DefaultMeshParent;
	
	private static ImmediateMesh GenerateMesh(Orbit orbit, Range? trueAnomalyRange = null)
	{
		var orbitMesh = new ImmediateMesh();
		var orbitMaterial = new StandardMaterial3D();
			
		orbitMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		orbitMaterial.AlbedoColor = orbit.Color;
				
		orbitMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip, orbitMaterial);
		orbitMesh.SurfaceSetColor(orbit.Color);

		trueAnomalyRange ??= orbit.TrueAnomalyRange;
		var conicRange = trueAnomalyRange.Value;
		
		
		for (ushort i = 0; i < OrbitVerticesCount; i++)
		{
			var vertexPosition = orbit.PositionFromTrueAnomaly(conicRange.LerpBetween((double)i /(OrbitVerticesCount-1)));
			orbitMesh.SurfaceAddVertex((Vector3)vertexPosition);
		}
		orbitMesh.SurfaceEnd();
		return orbitMesh;
	}
	
	public static OrbitMesh CreateOrbitLine(Orbit orbit, Range? trueAnomalyRange = null)
	{
		var orbitMeshNode = OrbitMeshPrefab.Instantiate();

		if (orbit.Primary != null)
		{
			var orbitMesh = GenerateMesh(orbit, trueAnomalyRange);
			((MeshInstance3D)orbitMeshNode).Mesh = orbitMesh;
			orbit.Primary.AddChild(orbitMeshNode);
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

	public void UpdateOrbitLine(Orbit newOrbit, Range? trueAnomalyRange = null)
	{
		Mesh = GenerateMesh(newOrbit, trueAnomalyRange);
		
		if (newOrbit.Primary != GetParent() && newOrbit.Primary != null)
		{ 
			Reparent(newOrbit.Primary, false);
		}
		_orbit = newOrbit;
	}

	public void CreateMarker(string markerName, double trueAnomaly, string markerText)
	{
		if (_markers.ContainsKey(markerName))
		{
			UpdateMarker(markerName, trueAnomaly, markerText);
			return;
		}
		
		var markerPosition = new RelativePosition(_orbit.PositionFromTrueAnomaly(trueAnomaly), _orbit.Primary);
		
		var marker = OrbitMarker.CreateMarker(markerPosition, markerText);
		marker.Name = markerName;
		
		AddChild(marker);
		_markers.Add(markerName,marker);
	}

	public void UpdateMarker(string markerName, double trueAnomaly, string markerText)
	{
		if (!_markers.TryGetValue(markerName, out var marker)) return;
		var newPosition = _orbit.PositionFromTrueAnomaly(trueAnomaly);
		
		marker.TextBox.Text = markerText;
		marker.RelPosition = new RelativePosition(newPosition, _orbit.Primary);
	}

	public void DeleteMarker(string markerName)
	{
		if (!_markers.TryGetValue(markerName, out var marker)) return;
		marker.QueueFree();
		_markers.Remove(markerName);
	}
	
	public override void _Process(double delta)
	{
		 if (Visible != GlobalValues.UIVisible) Visible = GlobalValues.UIVisible;
	}
}
