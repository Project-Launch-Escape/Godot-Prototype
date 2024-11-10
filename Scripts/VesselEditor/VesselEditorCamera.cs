using Godot;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class VesselEditorCamera : Camera3D
{
    [Export]
    public Vector3 Origin;

    [ExportGroup("Camera Settings")]
    [Export]
    public float MinCameraDistance = 5f;
    
    [Export]
    public float MaxCameraDistance = 50f;

    [Export]
    public float MinCameraPitch = -80f;
    
    [Export]
    public float MaxCameraPitch = 80f;

    [Export]
    public float CameraSensitivity = 0.1f;

    private float _cameraYaw;
    private float _cameraPitch;
    private float _cameraZoom;

    public override void _Ready()
    {
        _cameraZoom = MinCameraDistance;
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
            case InputEventMouseMotion { ButtonMask: MouseButtonMask.Middle } inputEventMouseMotion:
                UpdateInputPan(inputEventMouseMotion);
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp or MouseButton.WheelDown } inputEventMouseButton:
                UpdateInputScroll(inputEventMouseButton);
                break;
        }
    }

    private void UpdateInputPan(InputEventMouseMotion inputEventMouseMotion)
    {
        _cameraYaw += Mathf.DegToRad(-inputEventMouseMotion.Relative.X * CameraSensitivity) % Mathf.Pi;
        _cameraPitch += Mathf.DegToRad(inputEventMouseMotion.Relative.Y * CameraSensitivity);
        _cameraPitch = Mathf.Clamp(_cameraPitch, Mathf.DegToRad(MinCameraPitch), Mathf.DegToRad(MaxCameraPitch));
    }

    private void UpdateInputScroll(InputEventMouseButton inputEventMouseButton)
    {
        _cameraZoom += inputEventMouseButton.ButtonIndex == MouseButton.WheelUp ? -1 : 1;
        _cameraZoom = Mathf.Clamp(_cameraZoom, MinCameraDistance, MaxCameraDistance);
    }

    private void SetCameraTransform(float yaw, float pitch)
    {
        var rotation = new Quaternion(new Vector3(0, 1, 0), yaw) * new Quaternion(new Vector3(1, 0, 0), -pitch);
        var position = Origin + rotation * new Vector3(0, 0, _cameraZoom);
        
        SetPosition(position);
        SetQuaternion(rotation);
    }
}