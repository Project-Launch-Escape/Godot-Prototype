
using Godot;

namespace GodotPrototype.Scripts.VesselEditor;

public interface IToolable
{
    public bool IsToolActive { get; set; } // Property is true during tool usage (e.g. when a part is in the cursor for place tool), not when the tool is just enalbed
    // To find when a tool is enabled, use the Editor.EditorNode.IsToolEnabled(EditorTool) method

    public void HandleTool(InputEventMouseButton mouseInput, bool shiftModifier, bool altModifier);

    public void OnToolEnable();
    public void OnToolDisable();
}