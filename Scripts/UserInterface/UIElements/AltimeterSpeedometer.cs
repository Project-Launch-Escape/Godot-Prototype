using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class AltimeterSpeedometer : Control
{
	[Export] private Label _altitudeText;
	[Export] private Label _velocityText;
	[Export] private Label _verticalSpeedText;

	[Export] private TextureButton _trackingSwitchButton;
	[Export] private TextureRect _referenceIcon;
	
	public AltitudeType AltType = AltitudeType.Surface;

	private TrackingObjectType _trackingObjectType => _trackingSwitchButton.ButtonPressed ? 
		TrackingObjectType.Freecam : TrackingObjectType.Vessel;

	public override void _Process(double delta)
	{
		UpdateAltitudeText();
		UpdateVelocityText();
		UpdateVerticalSpeedText();
		UpdateReferenceIcon();
	}

	public void UpdateAltitudeText()
	{
		var altitude = _trackingObjectType is TrackingObjectType.Vessel ? 
			Vessel.ActiveVessel.PositionLocal.Magnitude : FlightCamera.PositionRel.LocalPosition.Magnitude;
		
		if (AltType is AltitudeType.Surface) altitude -= (_trackingObjectType is TrackingObjectType.Vessel ? Vessel.ActiveVessel.ParentBody: FlightCamera.ParentCelestial).Radius;

		var prefix = GlobalValues.ScalarToHighestSIPrefix(altitude);
		_altitudeText.Text = $"Alt: {altitude / prefix.prefixMultiplier:N2}{prefix.prefixName}m";
	}
	private void UpdateVelocityText()
	{
		var velocity =_trackingObjectType is TrackingObjectType.Vessel?
			Vessel.ActiveVessel.VelocityLocal.Magnitude : FlightCamera.ModifiedSpeed;

		var prefix = GlobalValues.ScalarToHighestSIPrefix(velocity);
		_velocityText.Text = $"Vel: {velocity / prefix.prefixMultiplier:N2}{prefix.prefixName}m/s";
	}
	
	private void UpdateVerticalSpeedText()
	{
		var velocity = _trackingObjectType is TrackingObjectType.Vessel ?
			Vessel.ActiveVessel.VelocityLocal : FlightCamera.LocalVelocity;

		var verticalSpeed = velocity.Dot(_trackingObjectType is TrackingObjectType.Vessel?
			Vessel.ActiveVessel.PositionLocal.Normalized() : FlightCamera.PositionRel.LocalPosition.Normalized());

		var prefix = GlobalValues.ScalarToHighestSIPrefix(verticalSpeed);
		_verticalSpeedText.Text = $"VSpeed: {verticalSpeed / prefix.prefixMultiplier:N2}{prefix.prefixName}m/s";
	}

	private void UpdateReferenceIcon()
	{
		var referenceObject = _trackingObjectType is TrackingObjectType.Vessel
			? Vessel.ActiveVessel.ParentBody : FlightCamera.ParentCelestial;

		_referenceIcon.Texture = ((IDepictable)referenceObject).Icon;
		_referenceIcon.Modulate = ((IDepictable)referenceObject).IconColor;
	}

	private enum TrackingObjectType
	{
		Freecam,
		Vessel
	}
}

public enum AltitudeType
{
	Center,
	Surface
}
