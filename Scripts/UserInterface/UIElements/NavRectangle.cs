using Godot;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class NavRectangle : Control
{
    [Export] private Control _cameraAngleIcon;
    [Export] private Control _vesselAngleIcon;
    [Export] private Control _progradeAngleIcon;
    [Export] private Control _retrogradeAngleIcon;
    [Export] private Control _normalAngleIcon;
    [Export] private Control _antinormalAngleIcon;

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