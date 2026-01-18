using System.Text.Json;
using Godot;
using GodotPrototype.Scripts.Vessels;
using FileAccess = Godot.FileAccess;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public static class VesselFileTools
{
	private static readonly JsonSerializerOptions DefaultOptions = new()
	{
		WriteIndented = true,
		IncludeFields = true
	};
	public static void SavePartToFile(VesselPart part, bool includeDescendents = true)
	{
		var fileName = "vessel.json";
		
		var savePath = Editor.VesselFileDirectory + fileName;
		var save = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);

		var jsonReady = new PartTreeJSON(part);
		var jsonString = JsonSerializer.Serialize(jsonReady, DefaultOptions);
		
		GD.Print($"Json Stored Successfully at ({savePath})!");
		save.StoreLine(jsonString);
		save.Close();
	}

	public static void LoadVesselFromFile(string path)
	{
		var savePath = Editor.VesselFileDirectory + "vessel.json";
		var save = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
		var json = save.GetAsText();

		var partTreeJSON = JsonSerializer.Deserialize<PartTreeJSON>(json, DefaultOptions);

		var partTree = partTreeJSON.ToPartTree();
		Editor.EditorNode.AddChild(partTree.RootPart);
	}

	public static PackedScene GetPartSceneFromName(string partName)
	{
		var fileNames = DirAccess.GetFilesAt(Editor.PartDefinitionDirectory);
		foreach (var fileName in fileNames)
		{
			var filePath = Editor.PartDefinitionDirectory + fileName;
			var partDef = ResourceLoader.Load<PartDefinition>(filePath);
			if (partDef.Name == partName) return partDef.Scene;
		}

		return null;
	}
}
