using Godot;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class HoverIcon : Control
{
    public static readonly List<HoverIcon> HoverIcons = [];
    public bool IsHovered = false;
    public bool IsLocked = false;
    public virtual void OnHoverEnter()
    {
    }
    public virtual void OnHoverExit()
    {
    }
    public virtual void WhileHovered()
    {
    }

    protected void AddToIconList()
    {
        HoverIcons.Add(this);
    }
}