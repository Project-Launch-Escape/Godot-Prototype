using Godot;
using GodotPrototype.Scripts.Debug;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.Simulation.Physics;

public partial class Vessel : Node3D, IRenderable
{
	public RelativePosition PositionRel = new (new Vector3d(-15147490.0,-2714.0,0));
	public RelativeVelocity VelocityRel = new (new Vector3d(0.1,0.1,0.1));
	private Vector3d _acceleration = new();

	public Celestial ParentBody;
	public Trajectory Trajectory;

	public Vector3d PositionLocal
	{
		get => PositionRel.LocalPosition;
		set => PositionRel.LocalPosition = value;
	}
	public Vector3d VelocityLocal
	{
		get => VelocityRel.LocalVelocity;
		set => VelocityRel.LocalVelocity = value;
	}

	[Export] public double Mass;
	[Export] public double Thrust;

	public double Elasticity = 0.5;

	private bool _w;
	private bool _s;
	private bool _a;
	private bool _d;
	private bool _shift;
	private bool _alt;
	private bool _z;
	private bool _x;

	private DebugVector _gravityVector;
	private DebugVector _thrustVector;
	
	public override void _Ready()
	{
		GlobalValues.ReceiveVessels(this);
		GlobalValues.ActiveVessel = this;
		FlightCamera.AddToRenderSpaceUpdate(this);
		
		var parentBody = GlobalValues.CelestialDict["Luna"];
		PositionLocal = parentBody.GetSurfacePosition(Math.PI / 16, 1.23);
		
		Trajectory = new Trajectory(PositionLocal, VelocityLocal, parentBody, new Color(0.9f, 0.4f, 0.8f));
		PositionRel.ParentPosition = parentBody.RelPosition;
		VelocityRel.ReferenceVelocity = parentBody.RelVelocity;
		ParentBody = parentBody;

		_gravityVector = DebugVector.CreateVector(Vector3d.Zero, Vector3d.Zero, new Color(0, 1, 0), this);
		_thrustVector = DebugVector.CreateVector(Vector3d.Zero, Vector3d.Zero, new Color(1f, 0.85f, 0.1f), this);
		
		LocalSurface.OnSOIChange(parentBody);
	}
	
	public override void _Process(double delta)
	{
		if (GlobalValues.Paused) return;
		
		var dtPhys = delta * GlobalValues.TimeScale;
		
		UpdateRotation();
		
		if (GlobalValues.PhysicsMode is PhysicsType.Newton)
		{
			PhysicsUpdateNewton(dtPhys);
		}
		else
		{
			PhysicsUpdateKepler();
		}
		
		Trajectory.Update();
	}

	private void PhysicsUpdateNewton(double dtPhys)
	{
		var currentSOI = PositionRel.FindHighestSOI();
		if (ParentBody != currentSOI)
		{
			PositionRel.ConvertRef(currentSOI?.RelPosition);
			VelocityRel.ConvertRef(currentSOI?.RelVelocity);
			ParentBody = currentSOI;
		}

		if (currentSOI != null)
		{
			var gravity = -PositionLocal.Normalized() * ParentBody.Mu / PositionLocal.MagnitudeSquared();
			_acceleration += gravity;
			_gravityVector.UpdateVector(Vector3d.Zero, 100 * gravity, _gravityVector.Color, this);
		}

		var thrustVector = Vector3d.Zero;
		var throttle = ((_z ? 1 : 0) - (_x ? 1 : 0)) * ((_shift ? 5 : 1) / (_alt ? 5 : 1));
		thrustVector += Thrust * -(Vector3d)Basis.Y * throttle;
		
		AddForce(thrustVector);

		_thrustVector.UpdateVector(Vector3d.Zero, 100 * thrustVector, _thrustVector.Color, this);
		
		VelocityLocal += dtPhys * _acceleration;
		PositionRel.LocalPosition += VelocityLocal * dtPhys;

		_acceleration = Vector3d.Zero;
		
		Trajectory.SetFromStateVectors(PositionLocal, VelocityLocal, ParentBody);
		if (ParentBody != null && PositionLocal.Magnitude < ParentBody.Radius)
		{
			PositionLocal.Magnitude = ParentBody.Radius;
			var normalVector = PositionLocal.Normalized();
			VelocityLocal -= normalVector * VelocityLocal.Dot(normalVector) * (Elasticity + 1);
		}
	}
	

	private void PhysicsUpdateKepler()
	{
		var newPosRel = Trajectory.PositionCurrent();
		PositionRel.ParentPosition = newPosRel.ParentPosition;
		PositionRel.LocalPosition = newPosRel.LocalPosition;
		ParentBody = Trajectory.GetOrbitAtTime(GlobalValues.Time).Primary;

		var newVelRel = Trajectory.VelocityCurrent();
		VelocityRel.ReferenceVelocity = newVelRel.ReferenceVelocity;
		VelocityRel.LocalVelocity = newVelRel.LocalVelocity;
	}

	private void UpdateRotation()
	{
		if (!(_w || _a || _s || _d)) return;
		
		if (_w) Rotation = new Vector3(FlightCamera.Pitch + MathF.PI / 2, FlightCamera.Yaw, 0);
		//if (_a) Rotation = ((Vector3)FlightCamera.PointingDirection).Rotated(new Vector3(FlightCamera.Pitch, FlightCamera.Yaw, 0), MathF.PI / 2);
		if (_s) Rotation = new Vector3(FlightCamera.Pitch - MathF.PI / 2, FlightCamera.Yaw, 0);
		//if (_d) Rotation = new Vector3(FlightCamera.Pitch + MathF.PI / 2, FlightCamera.Yaw - MathF.PI / 2, 0);
	}

	public void AddForce(Vector3d force)
	{
		_acceleration += force / Mass;
	}
	public void AddImpulse(Vector3d impulse)
	{
		VelocityLocal += impulse / Mass;
	}

	public void RenderUpdate()
	{
		Position = (Vector3)PositionRel[CoordinateSpace.VesselSpace];
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventKey inputEventKey) return;
		switch (inputEventKey.Keycode)
		{
			case Key.W:
				_w = inputEventKey.Pressed;
				break;
			case Key.S:
				_s = inputEventKey.Pressed;
				break;
			case Key.A:
				_a = inputEventKey.Pressed;
				break;
			case Key.D:
				_d = inputEventKey.Pressed;
				break;
			case Key.Shift:
				_shift = inputEventKey.Pressed;
				break;
			case Key.Alt:
				_alt = inputEventKey.Pressed;
				break;
			case Key.Z:
				_z = inputEventKey.Pressed;
				break;
			case Key.X:
				_x = inputEventKey.Pressed;
				break;
		}
	}
}
