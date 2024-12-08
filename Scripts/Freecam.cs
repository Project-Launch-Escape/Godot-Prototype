using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;

namespace GodotPrototype.Scripts;

public partial class Freecam : Camera3D
{
	public static CelestialScript ParentCelestial;
	[Export] public CelestialScript StartingParentCelestial;
	[Export] private DebugUiController _debugUI;
	[Export] public Color OrbitColor;
	

	public static NestedPosition NestedPos = new ();
	public static Orbit Orbit;

	private static List<double> _celestialDists;
	private static SortedList<double,CelestialScript> _sortedCelestialDists = new ();
	
	public static double VelocityMultiplier = 100000000000f;
	[Export] public float ModifierSpeedMultiplier = 10f;

	[Export(PropertyHint.Range, "0.0,1.0")] public float Sensitivity = 0.25f;

	private Vector2 _mousePosition;

	private bool _right;
	private bool _left;
	private bool _forwards;
	private bool _backwards;
	private bool _up;
	private bool _down;

	private bool _shift;
	private bool _alt;

	private float _totalPitch;
	private float _totalYaw;

	private Vector3d _velocity = new ();

	public override void _Ready()
	{
		ParentCelestial = StartingParentCelestial;
		NestedPos = new NestedPosition(new Vector3d(), ParentCelestial);
	}

	private static List<double> GetCelestialDistances()
	{
		var distances = new List<double>();
		foreach (var celestial in GlobalValues.AllCelestials)
		{
			distances.Add(celestial.NestedPos[0].Magnitude());
		}

		return distances;
	}

	private static SortedList<double, CelestialScript> GetSortedCelestialDistances()
	{
		var sortedDists = new SortedList<double, CelestialScript>();
		var celestials = GlobalValues.AllCelestials;
		for (int i = 0; i < celestials.Count; i++)
		{
			if (!celestials[i].Visible) continue;
			sortedDists.Add(_celestialDists[i], GlobalValues.AllCelestials[i]);
		}
		return sortedDists;
	}

	public static int GetDistanceIndex(CelestialScript celestial)
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
	
	private CelestialScript GetHighestSOI()
	{
		var celestials = GlobalValues.AllCelestials;
		var currentSOIs = new List<CelestialScript>();

		for (int i = 0; i < _celestialDists.Count; i++)
		{
			if (_celestialDists[i] <= celestials[i].SOIRadius)
			{
				currentSOIs.Add(celestials[i]);
			}
		}
		
		_debugUI.UpdateSOIs(currentSOIs);
		if (currentSOIs.Count == 0)
		{
			return null;
		}
		
		var highestSOILayer = CoordinateSpace.GalaxySpace;
		var highestSOIIndex = 0;
		
		for (var i = 0; i < currentSOIs.Count; i++)
		{
			if (currentSOIs[i].NestedPos.CoordLayer <= highestSOILayer) continue;
			highestSOILayer = currentSOIs[i].NestedPos.CoordLayer;
			highestSOIIndex = i;
		}
		
		return currentSOIs[highestSOIIndex];
	}
	
	private void SOIChange(CelestialScript newSOI)
	{
		var newRefPosition = new NestedPosition();
		var newCoordLayer = CoordinateSpace.GalaxySpace;
		if (newSOI != null)
		{
			newRefPosition = newSOI.NestedPos;
			newCoordLayer = newSOI.NestedPos.CoordLayer.Increment();
		}

		var newPosition = NestedPos.ConvertPositionReference(newRefPosition);
		
		NestedPos.LocalPosition = newPosition;
		NestedPos.CoordLayer = newCoordLayer;
		NestedPos.ParentPosition = newSOI?.NestedPos;
		ParentCelestial = newSOI;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_celestialDists = GetCelestialDistances();
		_sortedCelestialDists = GetSortedCelestialDistances();
		
		var highestSOI = GetHighestSOI();
		if (ParentCelestial != highestSOI) SOIChange(highestSOI);

		UpdateMouseLook();
		UpdateMovement(delta);
	}

	
	private void UpdateMovement(double delta)
	{
		var direction = new Vector3d((_right ? 1f : 0f) - (_left ? 1f : 0f), (_up ? 1f : 0f) - (_down ? 1f : 0f), (_backwards ? 1f : 0f) - (_forwards ? 1f : 0f));
		var speedMulti = 1f;
		if (_shift) speedMulti *= ModifierSpeedMultiplier;
		if (_alt) speedMulti /= ModifierSpeedMultiplier;

		if (direction == Vector3d.Zero)
		{
			_velocity = Vector3d.Zero;
		}
		else
		{
			_velocity = direction * VelocityMultiplier;
			Vector3 velocityRotated = ((Vector3)_velocity).Rotated(new Vector3(0f,1f,0f), Mathf.DegToRad(-_totalYaw));
			velocityRotated = velocityRotated.Rotated(new Vector3(1f,0f,0f).Rotated(new Vector3(0f,1f,0f), Mathf.DegToRad(-_totalYaw)).Normalized(), Mathf.DegToRad(-_totalPitch));
			
			NestedPos.LocalPosition += (Vector3d)velocityRotated * delta * speedMulti;
			
			Orbit = new Orbit(NestedPos.LocalPosition, velocityRotated, ParentCelestial, OrbitColor);
			//GD.Print($"a:{Orbit.a:F3}, b:{Orbit.b:F3}, e:{Orbit.e:F2}, w:{Orbit.w:F2}, i:{Orbit.i:F2}, l:{Orbit.l:F2}, n:{Orbit.n:F20}");
		}
	}

	private void UpdateMouseLook()
	{
		if (Input.GetMouseMode() != Input.MouseModeEnum.Captured) return;
		_mousePosition *= Sensitivity;
		var yaw = _mousePosition.X;
		var pitch = _mousePosition.Y;
		_mousePosition = new Vector2(0, 0);
		_totalPitch += pitch;
		RotateY(Mathf.DegToRad(-yaw));
		_totalYaw += yaw;
		RotateObjectLocal(new Vector3(1, 0, 0), Mathf.DegToRad(-pitch));
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion inputEventMouseMotion:
				_mousePosition = inputEventMouseMotion.Relative;
				break;
			case InputEventMouseButton inputEventMouseButton:
				switch (inputEventMouseButton.ButtonIndex)
				{
					case MouseButton.WheelUp:
						VelocityMultiplier *= 1.1f;
						break;
					case MouseButton.WheelDown:
						VelocityMultiplier /= 1.1f;
						break;
					case MouseButton.Right:
						Input.SetMouseMode(inputEventMouseButton.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible);
						break;
				}

				break;
			case InputEventKey inputEventKey:
				switch (inputEventKey.Keycode)
				{
					case Key.W:
						_forwards = inputEventKey.Pressed;
						break;
					case Key.A:
						_left = inputEventKey.Pressed;
						break;
					case Key.S:
						_backwards = inputEventKey.Pressed;
						break;
					case Key.D:
						_right = inputEventKey.Pressed;
						break;
					case Key.Q:
						_up = inputEventKey.Pressed;
						break;
					case Key.E:
						_down = inputEventKey.Pressed;
						break;
					case Key.Shift:
						_shift = inputEventKey.Pressed;
						break;
					case Key.Alt:
						_alt = inputEventKey.Pressed;
						break;
					case Key.O:
						OrbitLineMeshGenerator.CreateOrbitLine(Orbit);
						break;
				}

				break;
		}
	}
}
