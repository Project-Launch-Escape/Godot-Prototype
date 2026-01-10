using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.PLEDebug;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/VesselIcon.png")]
public partial class Vessel : RigidBody3D, IRenderable, IOrbitable
{
	public RelativePosition PositionRel = new (new Vector3d(-2514749.0,-2714.0,0));
	public RelativeVelocity VelocityRel = new (new Vector3d(50, 150, -1350));
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

	public double Throttle;
	
	public override void _Ready()
	{
		AllVessels.Add(this);
		ActiveVessel = this;
		FlightCamera.AddToRenderSpaceUpdate(this);
		
		FuelSystems.Add(new FuelSystem());
		InitializePartsFromFile("res://Vessels/save.tscn");
		
		var parentBody = Celestial.CelestialDict["Luna"];
		//PositionLocal = parentBody.GetSurfacePosition(0, Math.PI / 2);
		//VelocityLocal = Vector3d.One;
		
		Trajectory = new Trajectory(PositionLocal, VelocityLocal, parentBody, new Color(0.9f, 0.4f, 0.8f));
		PositionRel.ParentPosition = parentBody.PositionRel;
		VelocityRel.ReferenceVelocity = parentBody.VelocityRel;
		ParentBody = parentBody;

		_gravityVector = DebugVector.CreateVector(Vector3d.Zero, Vector3d.Zero, new Color(0, 1, 0), this);
		_thrustVector = DebugVector.CreateVector(Vector3d.Zero, Vector3d.Zero, new Color(1f, 0.85f, 0.1f), this);
		
		LocalSurface.OnSOIChange(parentBody);
		GlobalValues.FOPosition = new RelativePosition(PositionRel);
		
		Position = Vector3.Zero;
		LinearVelocity = (Vector3)VelocityLocal;
		ContactMonitor = true;
		MaxContactsReported = 10;
	}

	private void InitializePartsFromFile(string filePath)
	{
		RootPart = ResourceLoader.Load<PackedScene>(filePath).Instantiate<VesselPart>();
		RootPart.Position = Vector3.Zero;
		AddChild(RootPart);
		
		Parts = RootPart.GetAllDescendantParts();
		Parts.Add(RootPart);
		foreach (var part in Parts)
		{
			part.ParentVessel = this;
			part.ConnectedFuelSystem = FuelSystems[0];
			part.InitializePart();
			foreach (var partChild in part.GetChildren())
			{
				if (partChild is CollisionShape3D) partChild.Reparent(this);
			}
		}
	}

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		//if (state.GetContactCount() != 0) GD.Print($"V: {state.LinearVelocity.Length()}, ω: {state.AngularVelocity.Length()}, Col: {((Node)state.GetContactColliderObject(0)).Name}");
	}

	public override void _Process(double delta)
	{
		var dtPhys = delta;
		delta /= Engine.TimeScale;
		
		if (Input.IsActionJustReleased(PLEInput.PhysicsMode))
		{
			PhysicsMode = PhysicsMode is PhysicsType.Kepler ? PhysicsType.Newton : PhysicsType.Kepler;
			DebugUIController.UpdatePhysicsMode(PhysicsMode);
		}
		
		Throttle += delta * ((Input.IsActionPressed(PLEInput.ThrottleUp) ? 1 : 0) - (Input.IsActionPressed(PLEInput.ThrottleDown) ? 1 : 0)) * PLEInput.GetActiveModifier(5d);
		Throttle = Math.Clamp(Throttle, 0d, 1d);
		
		UpdateRotation();
		
		if (PhysicsMode is PhysicsType.Newton && !GlobalValues.Paused) PhysicsUpdateNewton(dtPhys);
		else PhysicsUpdateKepler();

		if (Position.Length() > 1000)
		{
			GlobalValues.FOPosition.LocalPosition += Position;
			Position = Vector3.Zero;
		}
		VelocityLocal = LinearVelocity;
		if (PositionRel.ParentPosition != GlobalValues.FOPosition.ParentPosition) GlobalValues.FOPosition.ConvertRef(PositionRel.ParentPosition);

		Trajectory.Update();
	}

	private void PhysicsUpdateNewton(double dtPhys)
	{
		Mass = (float)GetVesselMass();
		
		var currentSOI = PositionRel.FindHighestSOI();
		if (ParentBody != currentSOI)
		{
			PositionRel.ConvertRef(currentSOI?.PositionRel);
			VelocityRel.ConvertRef(currentSOI?.VelocityRel);
			ParentBody = currentSOI;
		}
		PositionLocal = GlobalValues.FOPosition.LocalPosition + Position;

		if (currentSOI != null)
		{
			var gravity = -PositionLocal.Normalized() * ParentBody.Mu / PositionLocal.MagnitudeSquared();
			AddForce(Mass * gravity);
			_gravityVector.UpdateVector(Vector3d.Zero, 100 * gravity, _gravityVector.Color, this);
		}

		var velprev = new Vector3d(LinearVelocity);
		foreach (var engine in Engines)
		{
			engine.FireEngine(Throttle, dtPhys);
		}
		
		var engineAccel = (LinearVelocity - velprev) / dtPhys;

		_thrustVector.UpdateVector(Vector3d.Zero, engineAccel, _thrustVector.Color, this);
		Trajectory.SetFromStateVectors(PositionLocal, VelocityLocal, ParentBody);
	}
	

	private void PhysicsUpdateKepler()
	{
		var newPosRel = Trajectory.PositionCurrent();
		PositionRel.ParentPosition = newPosRel.ParentPosition;
		PositionLocal = newPosRel.LocalPosition;
		ParentBody = Trajectory.GetOrbitAtTime(GlobalValues.Time).Primary;

		var newVelRel = Trajectory.VelocityCurrent();
		VelocityRel.ReferenceVelocity = newVelRel.ReferenceVelocity;
		LinearVelocity = (Vector3)newVelRel.LocalVelocity;

		Position = (Vector3)(PositionLocal - GlobalValues.FOPosition.LocalPosition);
	}

	private void UpdateRotation()
	{
		AngularVelocity = Vector3.Zero;
		if (Input.IsActionPressed(PLEInput.Forward)) Basis = GlobalValues.RenderSpaceCamera.GlobalBasis.Rotated(GlobalValues.RenderSpaceCamera.GlobalBasis.X, -Mathf.Pi/2);
		if (Input.IsActionPressed(PLEInput.Backward)) Basis = GlobalValues.RenderSpaceCamera.GlobalBasis.Rotated(GlobalValues.RenderSpaceCamera.GlobalBasis.X, Mathf.Pi/2);
	}

	public void AddForce(Vector3d force)
	{
		ApplyCentralForce((Vector3)force);
	}
	public void AddImpulse(Vector3d impulse)
	{
		ApplyCentralImpulse((Vector3)impulse);
	}

	private double GetVesselMass() => Parts.Sum(part => part.Mass);

	public void RenderUpdate()
	{
		//Position = (Vector3)PositionRel[CoordinateSpace.VesselSpace];
	}
}
