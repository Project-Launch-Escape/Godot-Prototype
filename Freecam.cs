using Godot;
using System;

public partial class Freecam : Camera3D
{
	[Export] public float VelocityMultiplier = 4;
	[Export] public float Acceleration = 30;
	[Export] public float Deceleration = 50;

	[Export] public float ModifierSpeedMultiplier = 2.5f;

	[Export(PropertyHint.Range,"0.0,1.0")] public float Sensitivity = 0.25f;

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


	public Vector3 Velocity;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateMouseLook();
		UpdateMovement((float)delta);
	}

	public void UpdateMovement(float delta)
	{
		var direction = new Vector3((Right ? 1f : 0f) - (Left ? 1f : 0f),(Up ? 1f : 0f) - (Down ? 1f : 0f), (Backwards ? 1f : 0f) - (Forwards ? 1f : 0f));
		var offset = direction.Normalized() * Acceleration * VelocityMultiplier * delta +
					 Velocity.Normalized() * Deceleration * VelocityMultiplier * delta;
		var speedMulti = 1f;
		if (Shift) speedMulti *= ModifierSpeedMultiplier;
		if (Alt) speedMulti /= ModifierSpeedMultiplier;

		if (direction == Vector3.Zero && offset.LengthSquared() > Velocity.LengthSquared())
		{
			Velocity = Vector3.Zero;
		}
		else
		{
			Velocity.X = Mathf.Clamp(Velocity.X + offset.X, -VelocityMultiplier, VelocityMultiplier);
			Velocity.Y = Mathf.Clamp(Velocity.Y + offset.Y, -VelocityMultiplier, VelocityMultiplier);
			Velocity.Z = Mathf.Clamp(Velocity.Z + offset.Z, -VelocityMultiplier, VelocityMultiplier);
			Translate(Velocity * delta * speedMulti);
		}

	}
	
	public void UpdateMouseLook()
	{
		if (Input.GetMouseMode() == Input.MouseModeEnum.Captured)
		{
			MousePosition *= Sensitivity;
			var yaw = MousePosition.X;
			var pitch = MousePosition.Y;
			MousePosition = new Vector2(0, 0);
			pitch = Mathf.Clamp(pitch, -90 - TotalPitch, 90 - TotalPitch);
			TotalPitch += pitch;
			RotateY(Mathf.DegToRad(-yaw));
			RotateObjectLocal(new Vector3(1,0,0), Mathf.DegToRad(-pitch));
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			MousePosition = inputEventMouseMotion.Relative;
		}
		
		if (@event is InputEventMouseButton inputEventMouseButton)
		{
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
		}

		if (@event is InputEventKey inputEventKey)
		{
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
		}
	}
}
