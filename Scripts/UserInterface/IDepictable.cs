using Godot;

namespace GodotPrototype.Scripts.UserInterface;

public interface IDepictable
{
    public Texture2D Icon { get; set; }
    public Color IconColor { get; set; }
}