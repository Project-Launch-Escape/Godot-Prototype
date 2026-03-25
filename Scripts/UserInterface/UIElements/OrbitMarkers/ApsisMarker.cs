using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

public partial class ApsisMarker : OrbitMarker
{
	[Export] private Control _normalDisplay;
	[Export] private Control _hoverDisplay;

	[Export] private Panel _hoverDisplayPanel;

	[Export] public Label HoverText1;
	[Export] public Label LabelText;
	[Export] public Label HoverText2;
	[Export] public Label HoverText3;

	[Export] private TextureButton _lockButton;

	private const float HoverPanelMinWidth = 128;
	public override void _Ready()
	{
		_normalDisplay.Visible = true;
		_hoverDisplay.Visible = false;

		AddToIconList();
		Modulate = ParentOrbit.Color;

		LabelText.Text = MarkerType switch
		{
			OrbitMarkerType.Apoapsis => "Ap",
			OrbitMarkerType.Periapsis => "Pe",
			_ => throw new ArgumentOutOfRangeException()
		};
		HoverText1.Text = MarkerType switch
		{
			OrbitMarkerType.Apoapsis => "Apoapsis",
			OrbitMarkerType.Periapsis => "Periapsis",
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public override void _Process(double delta)
	{
		Visible = GetVisibility() && ParentOrbit.OrbitLineNode.Visible;
		if (!Visible) return;
		TextProcess();
	}

	private bool GetVisibility()
	{
		switch (MarkerType)
		{
			case OrbitMarkerType.Apoapsis when ParentOrbit.IsEscapeTrajectory:
				return false;
			case OrbitMarkerType.Periapsis when !ParentOrbit.TrueAnomalyRange.ContainsValue(0):
				return false;
		}

		var renderSpacePosition = (Vector3)GetMarker3DRelPosition()[CoordinateSpace.RenderSpace];
		var behindCamera = Camera.IsPositionBehind(renderSpacePosition);
		
		if (behindCamera) return false;
		//if (!Visible) return false;

		Position = Camera.UnprojectPosition(renderSpacePosition);
		return true;
	}

	private void TextProcess()
	{
		HoverText3.Text = MarkerType switch
		{
			OrbitMarkerType.Apoapsis => $"{(ParentOrbit.Apoapsis - ParentOrbit.Primary.Radius) / 1000:N0}km",
			OrbitMarkerType.Periapsis => $"{(ParentOrbit.Periapsis - ParentOrbit.Primary.Radius) / 1000:N0}km",
			_ => HoverText3.Text
		};
		HoverText2.Text = MarkerType switch
		{
			OrbitMarkerType.Apoapsis => GetTimeString(ParentOrbit.TimeFromTrueAnomaly(Math.PI)),
			OrbitMarkerType.Periapsis => GetTimeString(ParentOrbit.TimeFromTrueAnomaly(0)),
			_ => HoverText2.Text
		};
	}

	private RelativePosition GetMarker3DRelPosition()
	{
		return new RelativePosition
		{
			ParentPosition = ParentOrbit.Primary.PositionRel,
			LocalPosition = MarkerType switch
			{
				OrbitMarkerType.Apoapsis => ParentOrbit.PositionFromTrueAnomaly(Math.PI),
				OrbitMarkerType.Periapsis => ParentOrbit.PositionFromTrueAnomaly(0d),
				_ => throw new ArgumentOutOfRangeException()
			}
		};

	}

	public override void OnHoverEnter()
	{
		_normalDisplay.Visible = false;
		_hoverDisplay.Visible = true;
	}
	public override void OnHoverExit()
	{
		_normalDisplay.Visible = true;
		_hoverDisplay.Visible = false;
	}

	public override void WhileHovered()
	{
		const int extraMargin = 15;
		var hoverPanelWidth = new List<float>
		{
			HoverText1.Size.X + extraMargin,
			HoverText2.Size.X + extraMargin,
			HoverText3.Size.X + extraMargin,
			HoverPanelMinWidth
		}.Max();
		_hoverDisplayPanel.Size = new Vector2(hoverPanelWidth, _hoverDisplayPanel.Size.Y);
		_hoverDisplayPanel.Position = new Vector2(-hoverPanelWidth / 2, _hoverDisplayPanel.Position.Y);
	
		_lockButton.ButtonPressed = IsLocked;
	}
}
