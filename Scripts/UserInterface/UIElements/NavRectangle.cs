using Godot;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class NavRectangle : Control
{
    [Export] private TextureButton _cameraAngleIcon;
    [Export] private TextureButton _vesselAngleIcon;
    [Export] private TextureButton _progradeAngleIcon;
    [Export] private TextureButton _retrogradeAngleIcon;
    [Export] private TextureButton _normalAngleIcon;
    [Export] private TextureButton _antinormalAngleIcon;
    [Export] private TextureButton _radialInAngleIcon;
    [Export] private TextureButton _radialOutAngleIcon;

    public SASType SASMode = SASType.Disabled;

    public enum SASType
    {
        Disabled,
        Camera,
        Assist,
        Prograde,
        Retrograde,
        Normal,
        Antinormal,
        RadialOut,
        RadialIn
    }

    public override void _Ready()
    {
        _cameraAngleIcon.Pressed += () => OnSASButtonPressed(_cameraAngleIcon, SASType.Camera);
        _vesselAngleIcon.Pressed += () => OnSASButtonPressed(_vesselAngleIcon, SASType.Assist);
        _progradeAngleIcon.Pressed += () => OnSASButtonPressed(_progradeAngleIcon, SASType.Prograde);
        _retrogradeAngleIcon.Pressed += () => OnSASButtonPressed(_retrogradeAngleIcon, SASType.Retrograde);
        _normalAngleIcon.Pressed += () => OnSASButtonPressed(_normalAngleIcon, SASType.Normal);
        _antinormalAngleIcon.Pressed += () => OnSASButtonPressed(_antinormalAngleIcon, SASType.Antinormal);
        if (_radialOutAngleIcon != null) _radialOutAngleIcon.Pressed += () => OnSASButtonPressed(_radialOutAngleIcon, SASType.RadialOut);
        if (_radialInAngleIcon != null) _radialInAngleIcon.Pressed += () => OnSASButtonPressed(_radialInAngleIcon, SASType.RadialIn);
    }

    private void OnSASButtonPressed(TextureButton sasButton, SASType sasType)
    {
        SetSASMode(sasType == SASMode ? SASType.Disabled : sasType);
    }

    public void SetSASMode(SASType sasType)
    {
        
    }

    public override void _Process(double delta)
    {
        UpdateIconFromDirection(_cameraAngleIcon, FlightCamera.PointingDirection);
        UpdateIconFromDirection(_vesselAngleIcon, Vessel.ActiveVessel.Basis.Y);
        UpdateIconFromDirection(_progradeAngleIcon, Vessel.ActiveVessel.VelocityLocal);
        UpdateIconFromDirection(_retrogradeAngleIcon, -Vessel.ActiveVessel.VelocityLocal);
        UpdateIconFromDirection(_normalAngleIcon, Vessel.ActiveVessel.Trajectory.CurrentOrbit.NormalVector);
        UpdateIconFromDirection(_antinormalAngleIcon, -Vessel.ActiveVessel.Trajectory.CurrentOrbit.NormalVector);
    }

    private void UpdateIconFromDirection(Control icon, Vector3d direction)
    {
        var sphericalCoords = SphericalCoordinates.FromCartesian((Vector3)direction * FlightCamera.CameraReferenceBasis);
        if (icon is null) return;
        var newAngles = new Vector2((float)sphericalCoords.Theta + Mathf.Pi, (float)sphericalCoords.Phi);
        var newPos = newAngles * Size.X / Mathf.Tau;
        newPos -= icon.Size / 2;
        icon.Position = newPos;
    }
}