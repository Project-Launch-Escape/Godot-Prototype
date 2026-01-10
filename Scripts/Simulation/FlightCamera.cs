using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.PLEDebug;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.Vessels;

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
	private static SortedList<double, Celestial> _sortedCelestialDists = new ();
	
	public static double Speed = 1000000f;
	private const double ModifierSpeedMultiplier = 5;
	public static double ModifiedSpeed => Speed * PLEInput.GetActiveModifier(ModifierSpeedMultiplier);

	[Export(PropertyHint.Range, "0.0,1.0")] private float _mouseSensitivity = 0.25f;
	private Vector2 _deltaMousePos;

	public static float Pitch;
	public static float Yaw;
	public static Vector2 FacingDirectionAngles => new (Yaw, Pitch);

	public static Vector3d PointingDirection => -GlobalValues.RenderSpaceCamera.GlobalBasis.Z;
	public static Vector3d LocalVelocity = new ();
	

	private static readonly List<IRenderable> RenderSpaceObjects = [];

	public static CameraReferenceType CameraReference = CameraReferenceType.Origin;
	public static Basis CameraReferenceBasis = Basis.Identity;
	
	public enum CameraModeType
	{
		Freecam = 0,
		Orbitcam = 1
	}

	public enum CameraReferenceType
	{
		Origin,
		Horizon
	}

	public override void _Ready()
	{
		
	}

	public static void AddToRenderSpaceUpdate(IRenderable renderObject)
	{
		RenderSpaceObjects.Add(renderObject);
	}

	private static List<double> GetCelestialDistances()
	{
		var distances = new List<double>();
		foreach (var celestial in Celestial.AllCelestials)
		{
			distances.Add(celestial.PositionRel[CoordinateSpace.RenderSpace].Magnitude);
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
		delta /= Engine.TimeScale;
		_celestialDists = GetCelestialDistances();
		_sortedCelestialDists = GetSortedCelestialDistances();
		
		UpdateMouseLook();
		
		var currentSOI = PositionRel.FindHighestSOI();
		if (ParentCelestial != currentSOI)
		{
			PositionRel.ConvertRef(currentSOI?.PositionRel);
			ParentCelestial = currentSOI;
		}
		
		if (CameraMode is CameraModeType.Freecam) FreecamUpdate(delta);
		else OrbitCamUpdate();

		foreach (var celestial in RenderSpaceObjects)
		{
			celestial.RenderUpdate();
		}
		GlobalValues.LocalSpaceCamera.Position = (Vector3)PositionRel[CoordinateSpace.VesselSpace];
	}

	private void OrbitCamUpdate()
	{
		var refPosition = _orbitCamCenter.PositionRefConverted(PositionRel.ParentPosition);
		PositionRel.LocalPosition = refPosition + _orbitCamRadius * (Vector3d)GlobalBasis.Z;
	}

	private void FreecamUpdate(double delta)
	{
		_orbitCamCenter = PositionRel.ParentPosition;

		var velocity = Vector3d.Zero;
		if (Input.IsActionPressed(PLEInput.Right)) velocity += GlobalBasis.X;
		if (Input.IsActionPressed(PLEInput.Left)) velocity -= GlobalBasis.X;
		if (Input.IsActionPressed(PLEInput.Up)) velocity += GlobalBasis.Y;
		if (Input.IsActionPressed(PLEInput.Down)) velocity -= GlobalBasis.Y;
		if (Input.IsActionPressed(PLEInput.Forward)) velocity -= GlobalBasis.Z;
		if (Input.IsActionPressed(PLEInput.Backward)) velocity += GlobalBasis.Z;
		if (velocity == Vector3d.Zero) return;
		
		velocity *= ModifiedSpeed;

		LocalVelocity = velocity;
			
		PositionRel.LocalPosition += velocity * delta;
	}
	
	private void UpdateMouseLook()
	{
		Yaw -= Mathf.DegToRad(_deltaMousePos.X);
		Pitch -= Mathf.DegToRad(_deltaMousePos.Y);
		_deltaMousePos = Vector2.Zero;
		
		Yaw %= Mathf.Tau;
		Pitch %= Mathf.Tau;

		switch (CameraReference)
		{
			case CameraReferenceType.Origin:
			{
				Basis = Basis.Identity;
				break;
			}
			case CameraReferenceType.Horizon:
			{
				var localPos = ParentCelestial != null? PositionRel.PositionRefConverted(ParentCelestial.PositionRel).Normalized() : PositionRel.LocalPosition.Normalized();
				var posRelSpherical = SphericalCoordinates.FromCartesian(localPos);
				var phi = (float)posRelSpherical.Phi;
				var theta = (float)posRelSpherical.Theta;
		
				var basisY = (Vector3)PositionRel.LocalPosition.Normalized();
				var basisZ = -new Vector3(Mathf.Cos(phi) * Mathf.Cos(theta), Mathf.Cos(phi) * Mathf.Sin(theta), -Mathf.Sin(phi)); // Points tangent to the surface along latitude lines
				var basisX = basisY.Cross(basisZ);
				Basis = new Basis(basisX, basisY, basisZ);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}
		CameraReferenceBasis = Basis;
		Basis = Basis.Rotated(Basis.Y, Yaw);
		Basis = Basis.Rotated(Basis.X, Pitch);

		GlobalValues.LocalSpaceCamera.Basis = Basis;
	}

	public override void _Input(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventMouseMotion inputEventMouseMotion:
				if (Input.IsMouseButtonPressed(MouseButton.Right)) _deltaMousePos = inputEventMouseMotion.Relative * _mouseSensitivity;
				break;
			case InputEventMouseButton {ButtonIndex: MouseButton.WheelUp}:
				if (CameraMode is CameraModeType.Freecam) Speed *= 1.1;
				else _orbitCamRadius /= 0.1 * PLEInput.GetActiveModifier(5d) + 1;
				break;
			case InputEventMouseButton {ButtonIndex: MouseButton.WheelDown}:
				if (CameraMode is CameraModeType.Freecam) Speed /= 1.1;
				else _orbitCamRadius *= 0.1 * PLEInput.GetActiveModifier(5d) + 1;
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
			case InputEventKey {Keycode: Key.L, Pressed: false}:
				CameraReference = CameraReference is CameraReferenceType.Origin
					? CameraReferenceType.Horizon : CameraReferenceType.Origin;
				break;
		}
	}
}
