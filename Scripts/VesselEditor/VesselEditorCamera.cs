using Godot;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class VesselEditorCamera : Camera3D
{
	public Vector3 Origin = Vector3.Zero;

	[ExportGroup("Camera Settings")]
	[Export]
	public float MinCameraZoom = 2f;
	
	[Export]
	public float MaxCameraZoom = 50f;
	
	[Export]
	public float StartCameraZoom = 5f;

	[Export]
	public float CameraSensitivity = 0.1f;
	
	[Export]
	public float MoveSpeed = 0.1f;

	private float _cameraYaw;
	private float _cameraPitch;
	private float _cameraZoom;
	private static bool Shift => Input.IsKeyPressed(Key.Shift);

	public override void _Ready()
	{
		_cameraZoom = StartCameraZoom;
		_cameraPitch = Mathf.DegToRad(30f);
	}

	public override void _Process(double delta)
	{
		SetCameraTransform(_cameraYaw, _cameraPitch);
	}

	public override void _Input(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventMouseMotion { ButtonMask: MouseButtonMask.Middle } inputEventMouseMotion when !Shift:
				UpdateInputPan(inputEventMouseMotion);
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp or MouseButton.WheelDown } inputEventMouseButton when !Shift:
				UpdateInputScroll(inputEventMouseButton);
				break;
			case InputEventMouseMotion { ButtonMask: MouseButtonMask.Middle} mouseMotion when Shift:
				UpdateInputMove(mouseMotion);
				break;
		}
	}

	private void UpdateInputPan(InputEventMouseMotion inputEventMouseMotion)
	{
		_cameraYaw += Mathf.DegToRad(-inputEventMouseMotion.Relative.X * CameraSensitivity) % Mathf.Pi;
		_cameraPitch += Mathf.DegToRad(inputEventMouseMotion.Relative.Y * CameraSensitivity);
	}

	private void UpdateInputScroll(InputEventMouseButton inputEventMouseButton)
	{
		_cameraZoom += inputEventMouseButton.ButtonIndex == MouseButton.WheelUp ? -1 : 1;
		_cameraZoom = Mathf.Clamp(_cameraZoom, MinCameraZoom, MaxCameraZoom);
	}

	private void UpdateInputMove(InputEventMouseMotion mouseMotion)
	{
		var adjustedSpeed = MoveSpeed * _cameraZoom;
		Origin += -Basis.X * mouseMotion.Relative.X * adjustedSpeed;
		Origin += Basis.Y * mouseMotion.Relative.Y * adjustedSpeed;
	}

	private void SetCameraTransform(float yaw, float pitch)
	{
		var rotation = new Quaternion(Vector3.Up, yaw) * new Quaternion(Vector3.Right, -pitch);
		var position = Origin + rotation * new Vector3(0, 0, _cameraZoom);
		
		SetPosition(position);
		SetQuaternion(rotation);
	}
}
