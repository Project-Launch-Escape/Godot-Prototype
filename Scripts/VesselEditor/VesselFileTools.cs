using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using GodotPrototype.Scripts.VesselEditor.EditorUI;
using GodotPrototype.Scripts.VesselEditor.FileInterfacing;
using GodotPrototype.Scripts.Vessels;
using FileAccess = Godot.FileAccess;

namespace GodotPrototype.Scripts.VesselEditor;

public static class VesselFileTools
{
	private static readonly JsonSerializerOptions DefaultOptions = new()
	{
		WriteIndented = true,
		IncludeFields = true,
	};
	public static void SavePartToFile(VesselPart part, bool includeDescendents = true)
	{
		var savePath = Editor.VesselFileDirectory + "vessel.json";
		var save = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);

		var jsonReady = new VesselPartJSON(part, 0, 0);
		var jsonString = JsonSerializer.Serialize(jsonReady, DefaultOptions);
		
		GD.Print(jsonString);
		save.StoreLine(jsonString);
		save.Close();
	}

	public static void LoadVesselFromFile(string path)
	{
		var savePath = Editor.VesselFileDirectory + "vessel.json";
		var save = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
		var json = save.GetAsText();

		var vesselPartJSON = JsonSerializer.Deserialize<VesselPartJSON>(json, DefaultOptions);

		var part = vesselPartJSON.ToPart();
		Editor.EditorNode.AddChild(part);
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
