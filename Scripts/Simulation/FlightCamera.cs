using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using DebugUIController = GodotPrototype.Scripts.Debug.DebugUIController;

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

	private bool _d;
	private bool _a;
	private bool _w;
	private bool _s;
	private bool _e;
	private bool _q;

	private bool _shift;
	private bool _alt;

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
		foreach (var celestial in GlobalValues.AllCelestials)
		{
			distances.Add(celestial.RelPosition[CoordinateSpace.RenderSpace].Magnitude);
		}
		
		return distances;
	}

	private static SortedList<double, Celestial> GetSortedCelestialDistances()
	{
		var sortedDists = new SortedList<double, Celestial>();
		var celestials = GlobalValues.AllCelestials;
		
		for (int i = 0; i < celestials.Count; i++)
		{
			if (!celestials[i].Visible) continue;
			sortedDists.Add(_celestialDists[i], GlobalValues.AllCelestials[i]);
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
		
		var direction = new Vector3d((_d ? 1f : 0f) - (_a ? 1f : 0f), (_e ? 1f : 0f) - (_q ? 1f : 0f), (_s ? 1f : 0f) - (_w ? 1f : 0f));
		if (direction == Vector3d.Zero) return;
		
		var speedMulti = 1f;
		if (_shift) speedMulti *= _modifierSpeedMultiplier;
		if (_alt) speedMulti /= _modifierSpeedMultiplier;
		
		_velocity = direction * Speed;
		var velocityRotated = ((Vector3)_velocity).Rotated(Vector3.Up, Yaw);
		velocityRotated = velocityRotated.Rotated(Vector3.Right.Rotated(Vector3.Up, Yaw).Normalized(), Pitch);
			
		PositionRel.LocalPosition += (Vector3d)velocityRotated * delta * speedMulti;
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

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			_deltaMousePos = inputEventMouseMotion.ScreenRelative;
		}
		else if (@event is InputEventMouseButton inputEventMouseButton)
		{
			switch (inputEventMouseButton.ButtonIndex) 
			{
				case MouseButton.WheelUp:
					if (CameraMode is CameraModeType.Freecam) Speed *= 0.1 * (_shift? 5 : 1) / (_alt? 5 : 1) + 1;
					else _orbitCamRadius /= 0.1 * (_shift? 5 : 1) * (_alt? 0.1 : 1) + 1;
					break;
				
				case MouseButton.WheelDown:
					if (CameraMode is CameraModeType.Freecam) Speed /= 0.1 * (_shift? 5 : 1) / (_alt? 5 : 1) + 1;
					else _orbitCamRadius *= 0.1 * (_shift? 5 : 1) / (_alt? 5 : 1) + 1;
					break;
				
				case MouseButton.Right:
					Input.SetMouseMode(inputEventMouseButton.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible);
					break;
			}
		}
		else if (@event is InputEventKey inputEventKey)
		{
			switch (inputEventKey.Keycode)
			{
				case Key.W:
					_w = inputEventKey.Pressed;
					break;
				case Key.A:
					_a = inputEventKey.Pressed;
					break;
				case Key.S:
					_s = inputEventKey.Pressed;
					break;
				case Key.D:
					_d = inputEventKey.Pressed;
					break;
				case Key.Q:
					_e = inputEventKey.Pressed;
					break;
				case Key.E:
					_q = inputEventKey.Pressed;
					break;
				case Key.Shift:
					_shift = inputEventKey.Pressed;
					break;
				case Key.Alt:
					_alt = inputEventKey.Pressed;
					break;
				case Key.F:
					if (inputEventKey.Pressed) break;
					if (CameraMode is CameraModeType.Freecam)
					{
						CameraMode = CameraModeType.Orbitcam;
						_orbitCamRadius = PositionRel.LocalPosition.Magnitude;
					}
					else
					{
						CameraMode = CameraModeType.Freecam;
					}
					break;
				case Key.V:
					if (inputEventKey.Pressed) break;
					_orbitCamCenter = GlobalValues.ActiveVessel.PositionRel;
					PositionRel.ConvertRef(_orbitCamCenter);
					break;

			}
		}
	}
}
