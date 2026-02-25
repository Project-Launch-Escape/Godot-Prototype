
using System.Text.Json.Serialization;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public class EditorFile
{
	public PartTreeJSON[] Assemblies;

	public EditorFile(VesselPart[] rootParts)
	{
		Assemblies = new PartTreeJSON[rootParts.Length];
		for (var i = 0; i < rootParts.Length; i++)
		{
			var rootPart = rootParts[i];
			Assemblies[i] = new PartTreeJSON(rootPart.ParentTree);
		}
	}

	[JsonConstructor]
	public EditorFile(PartTreeJSON[] assemblies)
	{
		Assemblies = assemblies;
	}

	public void LoadEditorFile()
	{
		foreach (var partTreeJson in Assemblies)
		{
			var partTree = partTreeJson.ToPartTree();
			PlaceTool.ToolNode.AddChild(partTree.RootPart);
		}
	}
}
