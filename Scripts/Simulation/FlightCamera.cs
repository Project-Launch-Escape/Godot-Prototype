using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.Vessels;
using GodotPrototype.Scripts.PLEDebug;

namespace GodotPrototype.Scripts.Simulation;

public partial class FlightCamera : Camera3D
{
	public static Celestial ParentCelestial;
	[Export] private Color _orbitColor;

	public static CameraModeType CameraMode;
	private static RelativePosition _orbitCamCenter;
	private static double _orbitCamRadius = 1000;

	public static RelativePosition PositionRel = new ();

	private static List<double> _celestialDists;
	private static SortedList<double,Celestial> _sortedCelestialDists = new ();
	
	public static double Speed = 100000000000f;
	[Export] private float _modifierSpeedMultiplier = 5f;

	[Export(PropertyHint.Range, "0.0,1.0")] private float _mouseSensitivity = 0.25f;
	private Vector2 _deltaMousePos;

	public static float Pitch;
	public static float Yaw;

	public static Vector3d PointingDirection => -Vector3d.FromSpherical(1, Yaw + Math.PI, -Pitch + 3 * Math.PI / 2);
	private static Vector3d _velocity = new ();

	private static List<IRenderable> _renderSpaceObjects = [];
	
	
	public enum CameraModeType
	{
		Freecam = 0,
		Orbitcam = 1
	}

	public override void _Ready()
	{

	}

	public static void AddToRenderSpaceUpdate(IRenderable renderObject)
	{
		_renderSpaceObjects.Add(renderObject);
	}

	private static List<double> GetCelestialDistances()
	{
		var distances = new List<double>();
		foreach (var celestial in Celestial.AllCelestials)
		{
			distances.Add(celestial.RelPosition[CoordinateSpace.RenderSpace].Magnitude);
		}
		
		return distances;
	}

	private static SortedList<double, Celestial> GetSortedCelestialDistances()
	{
		var sortedDists = new SortedList<double, Celestial>();
		var celestials = Celestial.AllCelestials;
		
		for (int i = 0; i < celestials.Count; i++)
		{
			if (!celestials[i].Visible) continue;
			sortedDists.Add(_celestialDists[i], Celestial.AllCelestials[i]);
		}
		return sortedDists;
	}

	public static int GetDistanceIndex(Celestial celestial)
	{
		for (int i = 0; i < _sortedCelestialDists.Count; i++)
		{
			if (_sortedCelestialDists.GetValueAtIndex(i) == celestial)
			{
				return i;
			}
		}
		return 0;
	}
	
	public override void _Process(double delta)
	{
		_celestialDists = GetCelestialDistances();
		_sortedCelestialDists = GetSortedCelestialDistances();
		
		UpdateMouseLook();
		
		var currentSOI = PositionRel.FindHighestSOI();
		if (ParentCelestial != currentSOI)
		{
			DebugUIController.UpdateSOI(currentSOI);
			PositionRel.ConvertRef(currentSOI?.RelPosition);
			ParentCelestial = currentSOI;
			LocalSurface.OnSOIChange(currentSOI);
		}
		
		if (CameraMode is CameraModeType.Freecam) FreecamUpdate(delta);
		else OrbitCamUpdate();
		
		GlobalValues.LocalSpaceCamera.Position = (Vector3)PositionRel[CoordinateSpace.VesselSpace];

		foreach (var celestial in _renderSpaceObjects)
		{
			celestial.RenderUpdate();
		}
	}

	private void OrbitCamUpdate()
	{
		var theta = Yaw + Math.PI;
		var phi = -Pitch + 1.5 * Math.PI;

		var refPosition = _orbitCamCenter.PositionRefConverted(PositionRel.ParentPosition);
		PositionRel.LocalPosition = refPosition + Vector3d.FromSpherical(_orbitCamRadius, theta, phi);
	}

	private void FreecamUpdate(double delta)
	{
		_orbitCamCenter = PositionRel.ParentPosition;

		var velocity = Vector3d.Zero;
		if (Input.IsActionPressed(PLEInput.Right)) velocity += Basis.X;
		if (Input.IsActionPressed(PLEInput.Left)) velocity -= Basis.X;
		if (Input.IsActionPressed(PLEInput.Up)) velocity += Basis.Y;
		if (Input.IsActionPressed(PLEInput.Down)) velocity -= Basis.Y;
		if (Input.IsActionPressed(PLEInput.Forward)) velocity -= Basis.Z;
		if (Input.IsActionPressed(PLEInput.Backward)) velocity += Basis.Z;
		if (velocity == Vector3d.Zero) return;
		
		velocity *= Speed * PLEInput.GetActiveModifier(_modifierSpeedMultiplier);
			
		PositionRel.LocalPosition += velocity * delta;
	}
	
	private void UpdateMouseLook()
	{
		if (Input.GetMouseMode() != Input.MouseModeEnum.Captured) return;
		
		_deltaMousePos *= _mouseSensitivity;
		
		Yaw -= Mathf.DegToRad(_deltaMousePos.X);
		Pitch -= Mathf.DegToRad(_deltaMousePos.Y);
		
		_deltaMousePos = Vector2.Zero;

		Rotation = new Vector3(Pitch, Yaw, 0);
		
		GlobalValues.LocalSpaceCamera.Rotation = Rotation;
	}

	public override void _Input(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventMouseMotion inputEventMouseMotion:
				_deltaMousePos = inputEventMouseMotion.ScreenRelative;
				break;
			case InputEventMouseButton {ButtonIndex: MouseButton.WheelUp}:
				if (CameraMode is CameraModeType.Freecam) Speed *= 0.1 * PLEInput.GetActiveModifier(5d) + 1;
				else _orbitCamRadius /= 0.1 * PLEInput.GetActiveModifier(5d) + 1;
				break;
			case InputEventMouseButton {ButtonIndex: MouseButton.WheelDown}:
				if (CameraMode is CameraModeType.Freecam) Speed /= 0.1 * PLEInput.GetActiveModifier(5d) + 1;
				else _orbitCamRadius *= 0.1 * PLEInput.GetActiveModifier(5d) + 1;
				break;
			case InputEventMouseButton {ButtonIndex: MouseButton.Right} inputEventMouseButton:
				Input.SetMouseMode(inputEventMouseButton.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible);
				break;
			case InputEventKey {Keycode: Key.F} inputEventKey:
				if (inputEventKey.Pressed) break;
				if (CameraMode is CameraModeType.Freecam)
				{
					CameraMode = CameraModeType.Orbitcam;
					_orbitCamRadius = PositionRel.LocalPosition.Magnitude;
					break;
				}
				CameraMode = CameraModeType.Freecam;
				break;
			case InputEventKey {Keycode: Key.V} inputEventKey:
				if (inputEventKey.Pressed) break;
				_orbitCamCenter = Vessel.ActiveVessel.PositionRel;
				PositionRel.ConvertRef(_orbitCamCenter);
				break;
		}
	}
}
