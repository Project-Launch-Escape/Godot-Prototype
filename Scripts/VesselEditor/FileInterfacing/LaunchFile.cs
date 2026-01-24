

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public class LaunchFile
{
    [Required] public string StartingCelestial;
    [Required] public bool StartOnSurface; // Starts in orbit if false
    public PartTreeJSON PartTree;

    public LaunchFile(VesselPart rootPart)
    {
        PartTree = new PartTreeJSON(rootPart);
    }

    [SetsRequiredMembers] [JsonConstructor]
    public LaunchFile(string startingCelestial, bool startOnSurface, PartTreeJSON partTree)
    {
        StartingCelestial = startingCelestial;
        StartOnSurface = startOnSurface;
        PartTree = partTree;
    }
}