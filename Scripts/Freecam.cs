using System.Runtime.ConstrainedExecution;
using Godot;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;

namespace GodotPrototype.Scripts;

public partial class Freecam : Camera3D
{
	public static Celestial ParentCelestial;
	[Export] private DebugUiController _debugUI;
	[Export] private Color _orbitColor;
	

	public static NestedPosition NestedPos = new ();
	private static Orbit _vesselOrbit = new ();

	private static List<double> _celestialDists;
	private static SortedList<double,Celestial> _sortedCelestialDists = new ();
	
	public static double VelocityMultiplier = 100000000000f;
	[Export] private float _modifierSpeedMultiplier = 10f;

	[Export(PropertyHint.Range, "0.0,1.0")] private float _mouseSensitivity = 0.25f;

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
		_vesselOrbit = new Orbit(NestedPos.LocalPosition, _velocity, ParentCelestial, _orbitColor);
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
	
	private Celestial GetHighestSOI()
	{
		var celestials = GlobalValues.AllCelestials;
		var currentSOIs = new List<Celestial>();

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
	
	private void SOIChange(Celestial newSOI)
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
		
		/* Code for finding encounters (In Progress):
		var relevantCelestials = GetRelevantCelestials();
		_debugUI.UpdateRelevantCelestials(relevantCelestials);
		var encounterTime = 0d;
		if (relevantCelestials.Count != 0) encounterTime = FindEncounterTime(_vesselOrbit, relevantCelestials[0].CelestialOrbit, relevantCelestials[0].SOIRadius);
		GD.PrintT(_vesselOrbit.TrueAnomalyFromTime(encounterTime), GlobalValues.TimeToYearDayString(encounterTime));
		*/ 
	}

	
	private void UpdateMovement(double delta)
	{
		var direction = new Vector3d((_right ? 1f : 0f) - (_left ? 1f : 0f), (_up ? 1f : 0f) - (_down ? 1f : 0f), (_backwards ? 1f : 0f) - (_forwards ? 1f : 0f));
		if (direction == Vector3d.Zero) return;
		
		var speedMulti = 1f;
		if (_shift) speedMulti *= _modifierSpeedMultiplier;
		if (_alt) speedMulti /= _modifierSpeedMultiplier;
		
		_velocity = direction * VelocityMultiplier;
		var velocityRotated = ((Vector3)_velocity).Rotated(new Vector3(0f,1f,0f), Mathf.DegToRad(-_totalYaw));
		velocityRotated = velocityRotated.Rotated(new Vector3(1f,0f,0f).Rotated(new Vector3(0f,1f,0f), Mathf.DegToRad(-_totalYaw)).Normalized(), Mathf.DegToRad(-_totalPitch));
			
		NestedPos.LocalPosition += (Vector3d)velocityRotated * delta * speedMulti;
			
		_vesselOrbit.SetOrbitFromStateVectors(NestedPos.LocalPosition, velocityRotated, ParentCelestial);
	}

	private List<Celestial> GetRelevantCelestials()
	{
		var relevantCelestials = new List<Celestial>();
		if (ParentCelestial == null) return relevantCelestials;
		
		foreach (var celestial in ParentCelestial.ChildCelestials)
		{
			var periapsisInRange = celestial.CelestialOrbit.Periapsis > _vesselOrbit.Periapsis - celestial.SOIRadius && 
								   celestial.CelestialOrbit.Periapsis < _vesselOrbit.Apoapsis + celestial.SOIRadius;
			var apoapsisInRange = celestial.CelestialOrbit.Apoapsis > _vesselOrbit.Periapsis - celestial.SOIRadius &&
								   celestial.CelestialOrbit.Apoapsis < _vesselOrbit.Apoapsis + celestial.SOIRadius;
			
			if (periapsisInRange || apoapsisInRange)
			{
				relevantCelestials.Add(celestial);
			}
		}
		return relevantCelestials;
	}

	private double GetCelestialDistanceAtTime(Orbit orbit1, Orbit orbit2, double time)
	{
		return orbit1.GetPositionAtTime(time, false).DistanceTo(orbit2.GetPositionAtTime(time, false));
	}

	private double FindEncounterTime(Orbit veselOrbit, Orbit celestialOrbit, double encounterDist)
	{
		var encounterTime = GlobalValues.Time;
		const double tolerance = 1E-4;
		var derivativeDelta = 1E-3 / celestialOrbit.n;
		// Solve for encounter time using Newtons Method
		
		for (int i = 0; i < 10; i++)
		{
			var y = GetCelestialDistanceAtTime(veselOrbit, celestialOrbit, encounterTime) - encounterDist;
			var yDerivative = (y - GetCelestialDistanceAtTime(veselOrbit, celestialOrbit, encounterTime + derivativeDelta)) / derivativeDelta;
			
			var dt = y / yDerivative;
			encounterTime -= dt;
			if (dt < tolerance) break;
		}
		return encounterTime;
	}
	
	private void UpdateMouseLook()
	{
		if (Input.GetMouseMode() != Input.MouseModeEnum.Captured) return;
		_mousePosition *= _mouseSensitivity;
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
						OrbitMesh.CreateOrbitLine(_vesselOrbit);
						break;
				}

				break;
		}
	}
}
