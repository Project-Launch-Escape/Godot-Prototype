using Godot;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts.VesselEditor;

[Icon("res://Resources/UITextures/VesselEditor/RerootIcon.png")]
public partial class RerootTool : Node3D, IToolable
{
    public static RerootTool ToolNode;
    
    public bool IsToolActive { get; set; }
    
    public override void _Ready()
    {
        if (ToolNode != null) GD.PrintErr("Multiple PlaceTool Nodes!");
        ToolNode = this;
    }
    
    public void HandleTool(InputEventMouseButton mouseInput, bool shiftModifier, bool altModifier)
    {
        if (mouseInput.Pressed) return;
        
        var selectedPart = PartHover.GetMouseHoveredPart();
        var selectedTree = selectedPart.ParentTree;
        
        if (shiftModifier)
        {
            GD.Print(selectedTree);
            return;
        }

        var newRoot = selectedPart;
        
        selectedTree.Reroot(newRoot);
    }

    public void OnToolEnable()
    {
        
    }

    public void OnToolDisable()
    {
        
    }
}