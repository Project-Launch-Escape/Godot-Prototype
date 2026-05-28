using Godot;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class NavRectangle : Control
{
	[Export] private Godot.Collections.Dictionary<SASType, TextureButton> _sasButtons = [];
	[Export] private Vector2 ButtonSizeUnselected = new (24,24);
	[Export] private Vector2 ButtonSizeSelected = new (36,36);

	public static SASType SASMode = SASType.Disabled;

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
		RadialIn,
		Maneuver
	}

	public override void _Ready()
	{
		foreach (var (sasType, sasIcon) in _sasButtons)
		{
			if (sasIcon is null) continue;
			sasIcon.Pressed += () => OnSASButtonPressed(sasIcon, sasType);
		}
	}

	private void OnSASButtonPressed(TextureButton sasButton, SASType sasType)
	{
		SetSASMode(sasType == SASMode ? SASType.Disabled : sasType);
	}

	public void SetSASMode(SASType sasType)
	{
		foreach (var (_, sasButton) in _sasButtons)
		{
			sasButton.Size = ButtonSizeUnselected;
		}

		if (_sasButtons.TryGetValue(sasType, out var button)) button.Size = ButtonSizeSelected;
		SASMode = sasType;
	}

	public override void _Process(double delta)
	{
		foreach (var (sasType, sasButton) in _sasButtons)
		{
			if (sasType is SASType.Maneuver && Vessel.ActiveVessel.Maneuvers.Count <= 0)
			{
				sasButton.Visible = false;
				continue;
			}
			UpdateIconFromDirection(sasButton, GetDirectionFromSASType(sasType));
		}
	}

	public static Vector3d GetDirectionFromSASType(SASType sasType)
	{
		return sasType switch
		{
			SASType.Disabled => Vector3d.Zero,
			SASType.Camera => FlightCamera.PointingDirection,
			SASType.Assist => Vessel.ActiveVessel.Basis.Y.Normalized(), //TODO: Implement last rotation thing
			SASType.Prograde => Vessel.ActiveVessel.VelocityLocal.Normalized(),
			SASType.Retrograde => -Vessel.ActiveVessel.VelocityLocal.Normalized(),
			SASType.Normal => Vessel.ActiveVessel.Trajectory.CurrentOrbit.NormalVector,
			SASType.Antinormal => -Vessel.ActiveVessel.Trajectory.CurrentOrbit.NormalVector,
			SASType.RadialOut => -Vessel.ActiveVessel.VelocityLocal.Cross(Vessel.ActiveVessel.Trajectory.CurrentOrbit.NormalVector).Normalized(),
			SASType.RadialIn => Vessel.ActiveVessel.VelocityLocal.Cross(Vessel.ActiveVessel.Trajectory.CurrentOrbit.NormalVector).Normalized(),
			SASType.Maneuver => Vessel.ActiveVessel.Maneuvers[0].DeltaVAligned.Normalized(),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private void UpdateIconFromDirection(Control icon, Vector3d direction)
	{
		var sphericalCoords = SphericalCoordinates.FromCartesian((Vector3)direction * FlightCamera.CameraReferenceBasis);
		if (icon is null) return;
		var newAngles = new Vector2((float)sphericalCoords.Theta + Mathf.Pi, (float)sphericalCoords.Phi);
		var newPos = newAngles * Size.X / Mathf.Tau;
		newPos -= icon.Size / 2;
		icon.Position = newPos;
		icon.Visible = true;
	}
}
