using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class OrbitMesh : MeshInstance3D
{
	public Orbit Orbit => Conic is null? _orbitStore : Conic.Orbit;
	private Orbit _orbitStore;
	public ConicPatch Conic;
	private Dictionary<OrbitMarkerType, OrbitMarker> _markers = [];

	private const ushort OrbitVerticesCount = 512;
	
	private static readonly PackedScene OrbitMeshPrefab = GD.Load<PackedScene>("res://Prefabs/orbit_mesh.tscn");
	public static Node DefaultMeshParent;
	
	private static ImmediateMesh GenerateMesh(Orbit orbit, Range? trueAnomalyRange = null, bool isManeuver = false)
	{
		var orbitMesh = new ImmediateMesh();
		var orbitMaterial = new StandardMaterial3D();
			
		orbitMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		orbitMaterial.AlbedoColor = orbit.Color;
		orbitMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
				
		orbitMesh.SurfaceBegin(isManeuver ? Mesh.PrimitiveType.Lines: Mesh.PrimitiveType.LineStrip, orbitMaterial);
		orbitMesh.SurfaceSetColor(orbit.Color);

		trueAnomalyRange ??= orbit.TrueAnomalyRange;
		var conicRange = trueAnomalyRange.Value;
		
		
		for (ushort i = 0; i < OrbitVerticesCount; i++)
		{
			var trueAnomaly = conicRange.LerpBetween((double)i / (OrbitVerticesCount - 1));
			if (orbit.OrbitType is ConicType.Elliptical && !orbit.IsEscapeTrajectory) trueAnomaly = orbit.TrueAnomalyFromAnomaly(trueAnomaly);
			
			var vertexPosition = orbit.PositionFromTrueAnomaly(trueAnomaly);
			orbitMesh.SurfaceAddVertex((Vector3)vertexPosition);
		}
		orbitMesh.SurfaceEnd();
		return orbitMesh;
	}
	
	public static OrbitMesh CreateOrbitLine(Orbit orbit, Range? trueAnomalyRange = null, bool isManeuver = false)
	{
		var orbitMeshNode = (OrbitMesh)OrbitMeshPrefab.Instantiate();

		if (orbit.Primary != null)
		{
			var orbitMesh = GenerateMesh(orbit, trueAnomalyRange, isManeuver);
			orbitMeshNode.Mesh = orbitMesh;
			orbit.Primary.AddChild(orbitMeshNode);
		}
		else
		{
			DefaultMeshParent.AddChild(orbitMeshNode);
		}
		
		orbitMeshNode._orbitStore = orbit;
		return orbitMeshNode;
	}
	public static OrbitMesh CreateOrbitLine(ConicPatch patch, Range? trueAnomalyRange = null, bool isManeuver = false)
	{
		var orbitLine = CreateOrbitLine(patch.Orbit, trueAnomalyRange, isManeuver);
		orbitLine.Conic = patch;
		return orbitLine;
	}

	public void DeleteOrbitLine()
	{
		foreach (var marker in _markers.Values)
		{
			HoverIcon.HoverIcons.Remove(marker);
			marker.QueueFree();
		}
		QueueFree();
	}

	public void UpdateOrbitLine(Orbit newOrbit, Range? trueAnomalyRange = null, bool isManeuver = false)
	{
		Mesh = GenerateMesh(newOrbit, trueAnomalyRange, isManeuver);
		
		if (newOrbit.Primary != GetParent() && newOrbit.Primary != null)
		{ 
			Reparent(newOrbit.Primary, false);
		}
		_orbitStore = newOrbit;
	}
	public void UpdateOrbitLine(ConicPatch newPatch, bool isManeuver = false)
	{
		UpdateOrbitLine(newPatch.Orbit, newPatch.GetTrueAnomalyRange(), isManeuver);
		Conic = newPatch;
	}

	public OrbitMarker CreateMarkerOfType(OrbitMarkerType markerType)
	{
		if (_markers.ContainsKey(markerType)) return null;
		
		var marker = OrbitMarker.CreateMarker(this, markerType);
		
		AddChild(marker);
		_markers.Add(markerType, marker);
		return marker;
	}

	public void DeleteMarker(OrbitMarkerType markerType)
	{
		if (!_markers.TryGetValue(markerType, out var marker)) return;
		marker.QueueFree();
		_markers.Remove(markerType);
	}

	private void SetMarkerVisibility(bool visible)
	{
		foreach (var marker in _markers.Values)
		{
			marker.Visible = visible;
		}
	}

	private void SetMarkerAlpha(float alpha)
	{
		foreach (var marker in _markers.Values)
		{
			marker.Modulate = marker.Modulate with{A = alpha};
		}
	}

	private void SetMaterialAlpha(float alpha)
	{
		var material = (StandardMaterial3D)Mesh.SurfaceGetMaterial(0);
		material.AlbedoColor = Orbit.Color with{A = alpha};
		Mesh.SurfaceSetMaterial(0, material);
	}
	
	public override void _Process(double delta)
	{
		if (Orbit == null) return;
		
		if (!GlobalValues.UIVisible)
		{
			Visible = false;
			SetMarkerVisibility(false);
		}
		else
		{
			FadeOutProcess();
		}
	}

	private void FadeOutProcess()
	{
		if (Orbit.OrbitType is ConicType.Static) return;
		var distanceToCamera = Orbit.Primary.PositionRel[CoordinateSpace.RenderSpace].Magnitude;

		const float fadeEnd = 15f;
		const float fadeStart = 3f;
		
		if (distanceToCamera > fadeEnd * (float)Orbit.Apoapsis)
		{
			if (!Visible) return;
			Visible = false;
			//SetMarkerVisibility(false);
			return;
		}
		if (distanceToCamera > fadeStart * (float)Orbit.Apoapsis)
		{
			var normalizedDist = (float)(distanceToCamera / Orbit.Apoapsis);
			var alpha = Mathf.Sqrt((fadeEnd - normalizedDist) / (fadeEnd - fadeStart));
			SetMarkerAlpha(alpha);
			SetMaterialAlpha(alpha);
			
			if (Visible) return;
			Visible = true;
			//SetMarkerVisibility(true);
			return;
		}
		if (distanceToCamera < (float)Orbit.Periapsis / fadeEnd)
		{
			if (!Visible) return;
			
			Visible = false;
			//SetMarkerVisibility(false);
			return;
		}
		if (distanceToCamera < (float)Orbit.Periapsis / fadeStart)
		{
			var normalizedDist = (float)(distanceToCamera / Orbit.Periapsis);
			var alpha = Mathf.Sqrt((1 - normalizedDist * fadeEnd) / (1 - fadeEnd / fadeStart));
			SetMarkerAlpha(alpha);
			SetMaterialAlpha(alpha);
			
			if (Visible) return;
			Visible = true;
			SetMarkerVisibility(true);
			return;
		}
		
		if (Visible) return;
		SetMaterialAlpha(1f);
		SetMarkerAlpha(1f);
		Visible = true;
		//SetMarkerVisibility(true);
	}
}
