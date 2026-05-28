using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.Physics;

namespace GodotPrototype.Scripts.UserInterface.UIElements;

public partial class ManeuverEditor : Draggable
{
	public Maneuver ParentManeuver;

	private const double CoarseStepDefault = 100;
	private const double FineStepDefault = 0.1;
	private const double ModifierMult = 10;

	private double CoarseStep => CoarseStepDefault * PLEInput.GetActiveModifier(ModifierMult);
	private double FineStep => FineStepDefault * PLEInput.GetActiveModifier(ModifierMult);

	[ExportGroup("General Elements")]
	[Export] private Label _timeUnilBurnLabel;
	[Export] private Label _totalDeltaVLabel;

	[Export] private Label _trueAnomalyLabel;
	[Export] private Slider _trueAnomalySlider;
	
	[ExportGroup("OrbitSelection UI")]

	[Export] private Button _prevOrbitButton;
	[Export] private Button _nextOrbitButton;
	[Export] private Label _currentCelestialLabel;
	[Export] private TextureRect _currentCelestialTexture;
	
	[ExportGroup("AdjustUI Element Arrays")]
	[Export] private Label[] _deltaVLabels;
	[Export] private Label[] _coarseAdjustLabels;
	[Export] private Button[] _coarseAdjustPosButtons;
	[Export] private Button[] _coarseAdjustNegButtons;
	[Export] private Label[] _fineAdjustLabels;
	[Export] private Button[] _fineAdjustPosButtons;
	[Export] private Button[] _fineAdjustNegButtons;

	public static ManeuverEditor CreateEditor(Maneuver parentManeuver)
	{
		var editor = FlightUIController.ControllerNode.ManeuverEditorScene.Instantiate<ManeuverEditor>();
		editor.ParentManeuver = parentManeuver;
		FlightUIController.ControllerNode.AddChild(editor);
		return editor;
	}

	public override void _Ready()
	{
		_trueAnomalySlider.ValueChanged += v => ParentManeuver.BurnTrueAnomaly = v;
		_trueAnomalySlider.MinValue = -Mathf.Pi;
		_trueAnomalySlider.MaxValue = Mathf.Pi;

		_prevOrbitButton.Pressed += () => ParentManeuver.OrbitIndex = Math.Max(0, ParentManeuver.OrbitIndex - 1);
		_nextOrbitButton.Pressed += () => ParentManeuver.OrbitIndex = Math.Min(ParentManeuver.ParentTrajectory.ConicPatches.Count-1, ParentManeuver.OrbitIndex + 1);

		for (int i = 0; i < 3; i++)
		{
			ReadyAxisButtons(i, _coarseAdjustPosButtons[i], _coarseAdjustNegButtons[i], _fineAdjustPosButtons[i], _fineAdjustNegButtons[i]);
		}
	}

	private void ReadyAxisButtons(int iAxis, Button coursePos, Button courseNeg, Button finePos, Button fineNeg)
	{
		coursePos.Pressed += () => OnAdjustButtonPressed(iAxis, false, true);
		courseNeg.Pressed += () => OnAdjustButtonPressed(iAxis, true, true);
		
		finePos.Pressed += () => OnAdjustButtonPressed(iAxis, false, false);
		fineNeg.Pressed += () => OnAdjustButtonPressed(iAxis, true, false);
	}

	public override void _Process(double delta)
	{
		if (!Visible) return;

		_timeUnilBurnLabel.Text = GlobalValues.TimeToTMinusString(ParentManeuver.BurnTime);
		_totalDeltaVLabel.Text = $"{ParentManeuver.DeltaVOrbital.Magnitude:F2} m/s";

		_trueAnomalyLabel.Text = $"True Anomaly:\n{(Mathf.RadToDeg(ParentManeuver.BurnTrueAnomaly)):F2} degrees";

		for (int i = 0; i < 3; i++)
		{
			UpdateAxis(i, _deltaVLabels[i], _coarseAdjustLabels[i], _fineAdjustLabels[i]);
		}

		_currentCelestialLabel.Text = ParentManeuver.ParentOrbit.Primary.Name;
		_currentCelestialTexture.Texture = ParentManeuver.ParentOrbit.Primary.Icon;
		_currentCelestialTexture.Modulate = ParentManeuver.ParentOrbit.Primary.IconColor;

		var trueAnomalyRange = ParentManeuver.ParentConic.GetTrueAnomalyRange();
		_trueAnomalySlider.MinValue = trueAnomalyRange.MinValue;
		_trueAnomalySlider.MaxValue = trueAnomalyRange.MaxValue;
		
		ParentManeuver.CalculateTrajectory();
	}

	private void UpdateAxis(int iAxis, Label dvText, Label courseAdjustText, Label fineAdjustText)
	{
		dvText.Text = $"{ParentManeuver.DeltaVOrbital[iAxis]:F2} m/s";
		courseAdjustText.Text = $"{CoarseStep:F2} m/s";
		fineAdjustText.Text = $"{FineStep:F2} m/s";
	}

	private void OnAdjustButtonPressed(int iAxis, bool negative, bool coarse)
	{
		ParentManeuver.DeltaVOrbital[iAxis] += (negative ? -1 : 1) * (coarse ? CoarseStep : FineStep);
	}
}
