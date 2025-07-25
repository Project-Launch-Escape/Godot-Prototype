using Godot;
using GodotPrototype.Scripts.Simulation.DoublePrecision;

namespace GodotPrototype.Scripts.PLEDebug;

public partial class DebugVector : MeshInstance3D
{
	private static readonly PackedScene VectorPrefab = GD.Load<PackedScene>("res://Prefabs/debug_vector.tscn");

	public Color Color;
	
	private static ImmediateMesh GenerateMesh(Vector3d tailPos, Vector3d headPos, Color color)
	{
		var material = new StandardMaterial3D();
		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		material.AlbedoColor = color;
		
		var mesh = new ImmediateMesh();
		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
		
		mesh.SurfaceSetColor(color);
		
		mesh.SurfaceAddVertex((Vector3)tailPos);
		mesh.SurfaceAddVertex((Vector3)headPos);
		
		mesh.SurfaceEnd();
		return mesh;
	}
	
	public static DebugVector CreateVector(Vector3d tailPos, Vector3d headPos, Color color, Node3D parentObject)
	{
		var vectorNode = (MeshInstance3D)VectorPrefab.Instantiate();
		vectorNode.Mesh = GenerateMesh(tailPos, headPos, color);
		((DebugVector)vectorNode).Color = color;
		
		parentObject.AddChild(vectorNode);
		return (DebugVector)vectorNode;
	}
	
	public void UpdateVector(Vector3d tailPos, Vector3d headPos, Color color, Node3D parentObject)
	{
		Mesh = GenerateMesh(tailPos, headPos, color);
		Color = color;
		
		if (parentObject != GetParent() && parentObject != null)
		{ 
			Reparent(parentObject, false);
		}

		GlobalRotation = new Vector3();
	}
}
