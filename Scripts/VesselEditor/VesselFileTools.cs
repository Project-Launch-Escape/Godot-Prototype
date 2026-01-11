using Godot;
using GodotPrototype.Scripts.VesselEditor.EditorUI;
using GodotPrototype.Scripts.Vessels;
using FileAccess = Godot.FileAccess;

namespace GodotPrototype.Scripts.VesselEditor;

public static class VesselFileTools
{
    public static void SavePartToFile(VesselPart part, bool includeDescendents = true)
    {
        var savePath = Editor.VesselFileDirectory + "vessel.json";
        var save = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
        
        var jsonString = Json.Stringify(part, "\n", true, true);
        
        GD.Print(jsonString);
        save.StoreLine(jsonString);
        save.Close();
    }

    public static PackedScene GetPartSceneFromName(string partName)
    {
        var fileNames = DirAccess.GetFilesAt(Editor.PartSceneDirectory);
        foreach (var fileName in fileNames)
        {
            var filePath = Editor.PartSceneDirectory + fileName;
            var partDef = ResourceLoader.Load<PartDefinition>(filePath);
            if (partDef.PartName == partName) return partDef.PartScene;
        }

        return null;
    }
}