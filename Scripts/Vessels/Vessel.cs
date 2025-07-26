using Godot;
using GodotPrototype.Scripts.PLEDebug;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/VesselIcon.png")]
public partial class Vessel : Node3D, IRenderable
{
	public RelativePosition PositionRel = new (new Vector3d(-15147490.0,-2714.0,0));
	public RelativeVelocity VelocityRel = new (new Vector3d(50, 150, 450));
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

	public double Mass => GetVesselMass();
	public double Elasticity = 0.5;
	
	public PhysicsType PhysicsMode = PhysicsType.Kepler;

	private DebugVector _gravityVector;
	private DebugVector _thrustVector;

	public VesselPart RootPart;
	public List<VesselPart> Parts = []; 
	public List<FuelSystem> FuelSystems = [];
	public List<RocketEngine> Engines = [];


	public static readonly List<Vessel> AllVessels = [];
	public static Vessel ActiveVessel;
		
	
	
	public override void _Ready()
	{
		AllVessels.Add(this);
		ActiveVessel = this;
		FlightCamera.AddToRenderSpaceUpdate(this);
		
		FuelSystems.Add(new FuelSystem());
		
		RootPart = ResourceLoader.Load<PackedScene>("res://Vessels/save.tscn").Instantiate<VesselPart>();
		RootPart.Position = Vector3.Zero;
		AddChild(RootPart);
		
		Parts = RootPart.GetAllDescendantParts();
		Parts.Add(RootPart);
		foreach (var part in Parts)
		{
			part.ParentVessel = this;
			part.ConnectedFuelSystem = FuelSystems[0];
			part.InitializePart();
		}

		
		var parentBody = Celestial.CelestialDict["Luna"];
		//PositionLocal = parentBody.GetSurfacePosition(Math.PI / 2, Math.PI / 2);
		
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
		if (Input.IsActionJustReleased(PLEInput.PhysicsMode))
		{
			PhysicsMode = PhysicsMode is PhysicsType.Kepler ? PhysicsType.Newton : PhysicsType.Kepler;
			DebugUIController.UpdatePhysicsMode(PhysicsMode);
		}
		if (GlobalValues.Paused) return;
		
		var dtPhys = delta * GlobalValues.TimeScale;
		
		UpdateRotation();
		
		if (PhysicsMode is PhysicsType.Newton)
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
		
		var throttle = ((Input.IsActionPressed(PLEInput.ThrottleUp) ? 1 : 0) - (Input.IsActionPressed(PLEInput.ThrottleDown) ? 1 : 0)) * PLEInput.GetActiveModifier(5d);

		var velprev = new Vector3d(VelocityLocal);
		foreach (var engine in Engines)
		{
			engine.FireEngine(throttle, dtPhys);
		}
		var engineAccel = (VelocityLocal - velprev) / dtPhys;

		_thrustVector.UpdateVector(Vector3d.Zero, engineAccel, _thrustVector.Color, this);
		
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
		if (Input.IsActionPressed(PLEInput.Forward)) Rotation = new Vector3(FlightCamera.Pitch - MathF.PI / 2, FlightCamera.Yaw, 0);
		if (Input.IsActionPressed(PLEInput.Backward)) Rotation = new Vector3(FlightCamera.Pitch + MathF.PI / 2, FlightCamera.Yaw, 0);
	}

	public void AddForce(Vector3d force)
	{
		_acceleration += force / Mass;
	}
	public void AddImpulse(Vector3d impulse)
	{
		VelocityLocal += impulse / Mass;
	}

	private double GetVesselMass() => Parts.Sum(part => part.Mass);

	public void RenderUpdate()
	{
		Position = (Vector3)PositionRel[CoordinateSpace.VesselSpace];
	}
}
