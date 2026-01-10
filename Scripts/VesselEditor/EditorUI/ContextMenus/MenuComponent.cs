using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;

public abstract partial class MenuComponent : FoldableContainer
{
    public VesselPart ParentPart;
    public PartComponent ParentComponent;
    [Export] protected VBoxContainer ComponentMenuContainer;
}