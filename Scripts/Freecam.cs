using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts;

public partial class Freecam : Camera3D
{
	public static CelestialScript ParentCelestial;
	[Export] public CelestialScript StartingParentCelestial;
	public static CoordinateSpace CoordLayer;
	public static NestedPosition NestedPos;
	[Export] public float VelocityMultiplier = 4;
	[Export] public float Acceleration = 30;
	[Export] public float Deceleration = 50;

	[Export] public float ModifierSpeedMultiplier = 2.5f;

	[Export(PropertyHint.Range, "0.0,1.0")] public float Sensitivity = 0.25f;

	[Export] public Vector2 MousePosition;

	public bool Right;
	public bool Left;
	public bool Forwards;
	public bool Backwards;
	public bool Up;
	public bool Down;

	public bool Shift;
	public bool Alt;

	public bool DoRotation;

	public float TotalPitch;
	public float TotalYaw;

	public Vector3 Velocity;

	public override void _Ready()
	{
		ParentCelestial = StartingParentCelestial;
		CoordLayer = ParentCelestial.CoordLayer.Increment();
		NestedPos = new NestedPosition(new Vector3(0, 0, 0), ParentCelestial);
	}

	
	private static CelestialScript GetHighestSoi()
	{
		var celestials = GlobalValues.AllCelestials;
		var currentSoIs = new List<CelestialScript>();

		foreach (var t in celestials)
		{
			var dist = t.NestedPos.LocalPosition.DistanceTo(NestedPosition.ConvertPositionReference(NestedPos, t.NestedPos));

			if (dist <= t.SOIRadius)
			{
				currentSoIs.Add(t);
			}
		}
		
		if (currentSoIs.Count == 0)
		{
			return null;
		}
		GD.Print(currentSoIs.Count);
		var highestSoiLayer = CoordinateSpace.GalaxySpace;
		var highestSoiIndex = 0;
		for (var i = 0; i < currentSoIs.Count; i++)
		{
			if (currentSoIs[i].CoordLayer <= highestSoiLayer) continue;
			highestSoiLayer = currentSoIs[i].CoordLayer;
			highestSoiIndex = i;
		}

		return currentSoIs[highestSoiIndex];
	}
	
	//private void SOIChange(CelestialScript NewSOI)
	//{
	//	LocalPos = (LocalPos - NewSOI.GetPosition(CoordLayer)) * GlobalValues.GetRefConversionFactor(CoordLayer, NewSOI.CoordLayer+1) + ParentCelestial.GetPosition(NewSOI.CoordLayer+1);
	//	ParentCelestial = NewSOI;
	//}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//	CelestialScript HighestSOI = GetHighestSOI();
		//	if (ParentCelestial != HighestSOI && HighestSOI != null)
		//	{
		//		
		//	}
		// SOI stuff is still broken
		UpdateMouseLook();
		UpdateMovement((float)delta);
	}

	public void UpdateMovement(float delta)
	{
		var direction = new Vector3((Right ? 1f : 0f) - (Left ? 1f : 0f), (Up ? 1f : 0f) - (Down ? 1f : 0f), (Backwards ? 1f : 0f) - (Forwards ? 1f : 0f));
		var offset = direction.Normalized() * Acceleration * VelocityMultiplier * delta +
		             Velocity.Normalized() * Deceleration * VelocityMultiplier * delta;
		var speedMulti = 1f;
		if (Shift) speedMulti *= ModifierSpeedMultiplier;
		if (Alt) speedMulti /= ModifierSpeedMultiplier;

		if (direction == Vector3.Zero)
		{
			Velocity = Vector3.Zero;
		}
		else
		{
			Velocity.X = Mathf.Clamp(Velocity.X + offset.X, -VelocityMultiplier, VelocityMultiplier);
			Velocity.Y = Mathf.Clamp(Velocity.Y + offset.Y, -VelocityMultiplier, VelocityMultiplier);
			Velocity.Z = Mathf.Clamp(Velocity.Z + offset.Z, -VelocityMultiplier, VelocityMultiplier);
			Vector3 velocityRotated = Velocity.Rotated(new Vector3(0f,1f,0f), Mathf.DegToRad(-TotalYaw));
			velocityRotated = velocityRotated.Rotated(new Vector3(1f,0f,0f).Rotated(new Vector3(0f,1f,0f), Mathf.DegToRad(-TotalYaw)), Mathf.DegToRad(-TotalPitch));
			NestedPos = new NestedPosition(NestedPos.LocalPosition + (velocityRotated * delta * speedMulti), ParentCelestial);
		}

	}

	private void UpdateMouseLook()
	{
		if (Input.GetMouseMode() != Input.MouseModeEnum.Captured) return;
		MousePosition *= Sensitivity;
		var yaw = MousePosition.X;
		var pitch = MousePosition.Y;
		MousePosition = new Vector2(0, 0);
		pitch = Mathf.Clamp(pitch, -90 - TotalPitch, 90 - TotalPitch);
		TotalPitch += pitch;
		RotateY(Mathf.DegToRad(-yaw));
		TotalYaw += yaw;
		RotateObjectLocal(new Vector3(1, 0, 0), Mathf.DegToRad(-pitch));
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion inputEventMouseMotion:
				MousePosition = inputEventMouseMotion.Relative;
				break;
			case InputEventMouseButton inputEventMouseButton:
				switch (inputEventMouseButton.ButtonIndex)
				{
					case MouseButton.WheelUp:
						VelocityMultiplier *= 1.1f;
						break;
					case MouseButton.WheelDown:
						VelocityMultiplier /= 1.1f;
						break;
					case MouseButton.Right:
						Input.SetMouseMode(inputEventMouseButton.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible);
						break;
				}

				break;
			case InputEventKey inputEventKey:
				switch (inputEventKey.Keycode)
				{
					case Key.W:
						Forwards = inputEventKey.Pressed;
						break;
					case Key.A:
						Left = inputEventKey.Pressed;
						break;
					case Key.S:
						Backwards = inputEventKey.Pressed;
						break;
					case Key.D:
						Right = inputEventKey.Pressed;
						break;
					case Key.Q:
						Up = inputEventKey.Pressed;
						break;
					case Key.E:
						Down = inputEventKey.Pressed;
						break;
					case Key.Shift:
						Shift = inputEventKey.Pressed;
						break;
					case Key.Alt:
						Alt = inputEventKey.Pressed;
						break;
				}

				break;
		}
	}
}