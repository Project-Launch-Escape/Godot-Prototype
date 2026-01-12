using Godot;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass]
public partial class PartDefinition : Resource, IDepictable
{
    public static List<PartDefinition> PartDefinitions = [];
    public static Dictionary<string, PartDefinition> PartDefByID = [];
    
    
    [Export] public PackedScene Scene;
    [Export] public string Name;
    [Export] public string ID; // In format: PARTTYPE_AlphaNumericDigits(4). ex: ENGINE_132h

    [Export] public Texture2D Icon { get; set; }
    [Export] public Color IconColor { get; set; }


    public static void AddDefinition(PartDefinition def)
    {
        PartDefinitions.Add(def);
        PartDefByID.Add(def.ID, def);
    }
}