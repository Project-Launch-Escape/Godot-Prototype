using Godot;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class VesselEditorCamera : Camera3D
{
	public Vector3 CameraPivot = Vector3.Zero;

	[ExportGroup("Camera Settings")]
	
	[Export] private float _minCameraZoom = 2f;
	[Export] private float _maxCameraZoom = 50f;
	[Export] private float _startCameraZoom = 5f;

	[Export] private float _cameraSensitivity = 0.1f;
	[Export] private float _moveSpeed = 0.1f;

	[Export] private Node3D _focusPoint;

	private float _cameraYaw;
	private float _cameraPitch;
	private float _cameraZoom;
	private static bool Shift => Input.IsKeyPressed(Key.Shift);

	public override void _Ready()
	{
		_cameraZoom = _startCameraZoom;
		_cameraPitch = Mathf.DegToRad(30f);
	}

	public override void _Process(double delta)
	{
		SetCameraTransform(_cameraYaw, _cameraPitch);
		_focusPoint.GlobalPosition = CameraPivot;
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
		_cameraYaw += Mathf.DegToRad(-inputEventMouseMotion.Relative.X * _cameraSensitivity) % Mathf.Pi;
		_cameraPitch += Mathf.DegToRad(inputEventMouseMotion.Relative.Y * _cameraSensitivity);
	}

	private void UpdateInputScroll(InputEventMouseButton inputEventMouseButton)
	{
		_cameraZoom += inputEventMouseButton.ButtonIndex == MouseButton.WheelUp ? -1 : 1;
		_cameraZoom = Mathf.Clamp(_cameraZoom, _minCameraZoom, _maxCameraZoom);
	}

	private void UpdateInputMove(InputEventMouseMotion mouseMotion)
	{
		var adjustedSpeed = _moveSpeed * _cameraZoom / 1000;
		CameraPivot += -Basis.X * mouseMotion.Relative.X * adjustedSpeed;
		CameraPivot += Basis.Y * mouseMotion.Relative.Y * adjustedSpeed;
	}

	private void SetCameraTransform(float yaw, float pitch)
	{
		Quaternion = new Quaternion(Vector3.Up, yaw) * new Quaternion(Vector3.Right, -pitch);
		Position = CameraPivot + Quaternion * new Vector3(0, 0, _cameraZoom);
	}
}
