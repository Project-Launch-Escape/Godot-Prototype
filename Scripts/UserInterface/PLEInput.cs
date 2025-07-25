using Godot;

namespace GodotPrototype.Scripts.UserInterface;

public static class PLEInput
{
    public static readonly StringName Forward = new("Forward");
    public static readonly StringName Backward = new("Backward");
    public static readonly StringName Up = new("Up");
    public static readonly StringName Down = new("Down");
    public static readonly StringName Right = new("Right");
    public static readonly StringName Left = new("Left");
    
    public static readonly StringName ThrottleUp = new("Throttle Up");
    public static readonly StringName ThrottleDown = new("Throttle Down");
    
    public static readonly StringName VesselFocus = new("Vessel Focus");
    public static readonly StringName CameraMode = new("Camera Mode");
    public static readonly StringName PhysicsMode = new("Physics Mode");
    
    public static readonly StringName ModifierUp = new("Modifier Up");
    public static readonly StringName ModifierDown = new("Modifier Down");

    public static readonly StringName SaveCraft = new("Save Craft");
    


    public static double GetActiveModifier(double maxModifier) =>
        (Input.IsActionPressed(ModifierUp) ? maxModifier : 1) /
        (Input.IsActionPressed(ModifierDown) ? maxModifier : 1);
}