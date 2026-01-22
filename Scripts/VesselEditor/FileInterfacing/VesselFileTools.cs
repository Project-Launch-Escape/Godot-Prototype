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
	public static void SaveEditorStateToFile(string path)
	{
		var save = FileAccess.Open(path, FileAccess.ModeFlags.Write);

		var jsonString = GetEditorFileJsonString(Editor.EditorNode.GetRootParts());
		
		GD.Print($"Json Stored Successfully at ({path})!");
		save.StoreLine(jsonString);
		save.Close();
	}

	public static string GetEditorFileJsonString(VesselPart[] rootParts)
	{
		var jsonReady = new EditorFile(rootParts);
		return JsonSerializer.Serialize(jsonReady, DefaultOptions);
	}

	public static void LoadEditorFileFromFilePath(string filePath)
	{
		var editorFile = GetEditorFileFromFilePath(filePath);
		Editor.EditorNode.ClearEditor();
		editorFile.LoadEditorFile();
	}
	public static EditorFile GetEditorFileFromFilePath(string filePath)
	{
		var save = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		var jsonString = save.GetAsText();
		return GetEditorFileFromJson(jsonString);
	}
	public static EditorFile GetEditorFileFromJson(string jsonString)
	{
		return JsonSerializer.Deserialize<EditorFile>(jsonString, DefaultOptions);
	}

	public static string GetPartTreeJsonString(VesselPart rootPart)
	{
		var jsonReady = new PartTreeJSON(rootPart);
		return JsonSerializer.Serialize(jsonReady, DefaultOptions);
	}
	public static VesselPart GetVesselRootFromJson(string jsonString)
	{
		var partTreeJSON = JsonSerializer.Deserialize<PartTreeJSON>(jsonString, DefaultOptions);

		var partTree = partTreeJSON.ToPartTree();
		return partTree.RootPart;
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
