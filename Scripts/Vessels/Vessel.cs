using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.PLEDebug;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.UserInterface.UIElements;
using GodotPrototype.Scripts.VesselEditor.FileInterfacing;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/VesselIcon.png")]
public partial class Vessel : RigidBody3D, IRenderable, IOrbiter
{
	public static readonly List<Vessel> AllVessels = [];
	public static Vessel ActiveVessel;
	
	
	public RelativePosition PositionRel = new (new Vector3d(-2514749.0,-2714.0,0));
	public RelativeVelocity VelocityRel = new (new Vector3d(50, 150, -1350));
	private Vector3d _acceleration = new();

	public Celestial ParentBody;
	public Trajectory Trajectory;
	
	public List<Maneuver> Maneuvers = [];

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

	public PartTree PartAssembly;
	public List<FuelSystem> FuelSystems = [];

	public double Throttle;
	
	public override void _Ready()
	{
		AllVessels.Add(this);
		ActiveVessel = this;
		FlightCamera.AddToRenderSpaceUpdate(this);
		
		FuelSystems.Add(new FuelSystem());
		InitializeFromLaunchFile();
		SnapPoint.SetGlobalVisibility(false);

		_gravityVector = DebugVector.CreateVector(Vector3d.Zero, Vector3d.Zero, new Color(0, 1, 0), this);
		_thrustVector = DebugVector.CreateVector(Vector3d.Zero, Vector3d.Zero, new Color(1f, 0.85f, 0.1f), this);
		
		LocalSurface.OnSOIChange(ParentBody);
		GlobalValues.FOPosition = new RelativePosition(PositionRel);
		
		Position = Vector3.Zero;
		LinearVelocity = (Vector3)VelocityLocal;
		ContactMonitor = true;
		MaxContactsReported = 10;

		var newManeuver = new Maneuver(Trajectory.ConicPatches[0], 0.754, new Vector3d(100, 50, 50));
		Maneuvers.Add(newManeuver);
		newManeuver.CalculateTrajectory();
	}

	private void InitializeFromLaunchFile()
	{
		var launchFile = VesselFileTools.GetLaunchFile();
		var partTree = launchFile.PartTree.ToPartTree();

		var root = partTree.RootPart;
		root.Position = Vector3.Zero;
		AddChild(root);

		PartAssembly = new PartTree(root, this);
		PartAssembly.InitializeParts();
		
		foreach (var part in PartAssembly.Parts)
		{
			foreach (var partChild in part.GetChildren())
			{
				if (partChild is CollisionShape3D collider)
				{
					AddChild(collider.Duplicate());
				}
			}
		}
		
		var parentBody = Celestial.CelestialDict[launchFile.StartingCelestial];
		if (launchFile.StartOnSurface)
		{
			PositionLocal = parentBody.GetSurfacePosition(0, Math.PI / 2);
			VelocityLocal = Vector3d.One;
		}
		else
		{
			var orbitRadius = parentBody.Radius * 3;
			var orbitalVelocity = 1.3*Math.Sqrt(parentBody.Mu / orbitRadius);
			PositionLocal = new Vector3d(orbitRadius, 0.01 * orbitRadius, 0.01 * orbitRadius);
			VelocityLocal = new Vector3d(0, -orbitalVelocity * 0.05, orbitalVelocity);
		}
		
		Trajectory = new Trajectory(PositionLocal, VelocityLocal, parentBody, new Color(0.9f, 0.4f, 0.8f));
		PositionRel.ParentPosition = parentBody.PositionRel;
		VelocityRel.ReferenceVelocity = parentBody.VelocityRel;
		ParentBody = parentBody;
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
		foreach (var fuelSystem in FuelSystems)
		{
			fuelSystem.HandleFuelRequests();
		}
		
		//var encounterTime = Trajectory.FindEncounter(Trajectory.CurrentOrbit, Celestial.CelestialDict["Luna"], new Range(GlobalValues.Time, GlobalValues.Time + Trajectory.CurrentOrbit.Period));
		//if (!double.IsNaN(encounterTime) && Engine.GetFramesDrawn() % 48 == 0) GD.Print(GlobalValues.TimeToVerboseString(encounterTime), "\n", encounterTime,"\n", GlobalValues.Time + Trajectory.CurrentOrbit.Period / 2,"\n");
	}

	private void PhysicsUpdateNewton(double dtPhys)
	{
		Mass = (float)PartAssembly.GetTotalMass();
		
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

		//var velprev = new Vector3d(LinearVelocity); //Disabled since fuel now drains asynchronously
		foreach (var engine in PartAssembly.Engines)
		{
			engine.FireEngine(Throttle, dtPhys);
		}
		
		//var engineAccel = (LinearVelocity - velprev) / dtPhys;

		//_thrustVector.UpdateVector(Vector3d.Zero, engineAccel, _thrustVector.Color, this);
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
		AngularVelocity = Vector3.Zero;
	}

	private void UpdateRotation()
	{
		//AngularVelocity = Vector3.Zero;
		//if (Input.IsActionPressed(PLEInput.Forward)) Basis = GlobalValues.RenderSpaceCamera.GlobalBasis.Rotated(GlobalValues.RenderSpaceCamera.GlobalBasis.X, -Mathf.Pi/2);
		//if (Input.IsActionPressed(PLEInput.Backward)) Basis = GlobalValues.RenderSpaceCamera.GlobalBasis.Rotated(GlobalValues.RenderSpaceCamera.GlobalBasis.X, Mathf.Pi/2);
		var torqueAxis = Vector3.Zero;
		float torqueMagnitude = 25f;
		bool manualControl = false;
		
		if (Input.IsActionPressed(PLEInput.Forward)) {torqueAxis += Basis.Z; manualControl = true;}
		if (Input.IsActionPressed(PLEInput.Backward)) {torqueAxis -= Basis.Z; manualControl = true;}
		if (Input.IsActionPressed(PLEInput.Right)) {torqueAxis += Basis.X; manualControl = true;}
		if (Input.IsActionPressed(PLEInput.Left)) {torqueAxis -= Basis.X; manualControl = true;}
		if (Input.IsActionPressed(PLEInput.Up)) {torqueAxis += Basis.Y; manualControl = true;}
		if (Input.IsActionPressed(PLEInput.Down)) {torqueAxis -= Basis.Y; manualControl = true;}

		if (manualControl)
		{
			ApplyTorque(torqueAxis.Normalized() * torqueMagnitude);
		}
		else
		{
			ApplyTorque(GetSASTorque());
		}
	}

	public Vector3d GetSASTarget()
	{
		return NavRectangle.GetDirectionFromSASType(NavRectangle.SASMode);
	}
	public Vector3 GetSASTorque()
	{
		float maxTorque = 25f;
		var sasTarget = (Vector3)GetSASTarget();
		var angleToTarget = sasTarget.AngleTo(Basis.Y);
		
		return sasTarget.Cross(-Basis.Y) * maxTorque;
	}

	public void AddForce(Vector3d force)
	{
		ApplyCentralForce((Vector3)force);
	}
	public void AddImpulse(Vector3d impulse)
	{
		ApplyCentralImpulse((Vector3)impulse);
	}

	public void RenderUpdate()
	{
		//Position = (Vector3)PositionRel[CoordinateSpace.VesselSpace];
	}
}
