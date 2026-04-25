using Godot;

namespace GodotPrototype.Scripts.Other;

public partial class VesselViewport : SubViewportContainer
{
	public override void _Process(double delta)
	{
		Size = GlobalValues.RenderSpaceCamera.GetWindow().Size;
		Position = Vector2.Zero;
	}
}
