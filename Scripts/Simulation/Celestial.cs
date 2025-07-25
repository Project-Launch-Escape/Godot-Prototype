using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;

namespace GodotPrototype.Scripts.Simulation;

[GlobalClass, Icon("res://Resources/Icons/CelestialIcon.png")]
public partial class Celestial : Node3D, IRenderable
{
	public double Mass;
	public double Radius;
	public double SOIRadius;

	public double Mu => Mass * GlobalValues.G;
	
	public Orbit CelestialOrbit;
	public RelativePosition RelPosition = new ();
	public RelativeVelocity RelVelocity = new ();
	public Celestial ParentCelestial => CelestialOrbit?.Primary;
	
	public readonly List<Celestial> ChildCelestials = [];
	public MeshInstance3D SurfaceNode;
	
	public DirectionalLight3D LightEmissionNode;
	public DirectionalLight3D LightEmissionNodeMirror; //Same as above but in LocalSpace scene
	public double Luminosity;
	
	
	public static readonly List<Celestial> AllCelestials = [];
	public static readonly Dictionary<string, Celestial> CelestialDict = [];
	
	
	
	public override void _Ready()
	{
		if (CelestialOrbit != null)
		{
			RelPosition = new RelativePosition(CelestialOrbit.PositionCurrent(), ParentCelestial);
			RelVelocity = new RelativeVelocity(CelestialOrbit.VelocityCurrent(), ParentCelestial);
			CelestialOrbit.CreateOrbitLine();
		}
		else
		{
			RelPosition = new RelativePosition(Transform.Origin);
			RelVelocity = new RelativeVelocity();
		}
		AllCelestials.Add(this);
		CelestialDict.Add(Name, this);
		FlightCamera.AddToRenderSpaceUpdate(this);
	}
	
	public override void _Process(double dt)
	{
		if (GlobalValues.Paused) return;
		if (CelestialOrbit != null)
		{
			RelPosition.LocalPosition = CelestialOrbit.PositionCurrent();
			RelVelocity.LocalVelocity = CelestialOrbit.VelocityCurrent();
		}
	}

	public Vector3d GetSurfacePosition(double latitude, double longitude) => new SphericalCoordinates(Radius, longitude, latitude - Math.PI / 2).ToCartesian();

	public void RenderUpdate()
	{
		var renderSpacePos = RelPosition[CoordinateSpace.RenderSpace];
		var newScale = 1 / renderSpacePos.Magnitude;

		var extraScale = 1;
		if (SurfaceNode != null)
		{
			SurfaceNode.Visible = newScale * Radius > 1e-4;
			extraScale = SurfaceNode.Visible ? FlightCamera.GetDistanceIndex(this) + 1 : 1;
		}

		Scale = Vector3.One * (float)newScale * extraScale;
		Position = (Vector3)renderSpacePos.AsMagnitude(extraScale);
		
		if (LightEmissionNode is not null) LightUpdate(renderSpacePos);
	}

	private void LightUpdate(Vector3d renderSpacePos)
	{ 
		var lux = 175 * (float)Math.Log10(Luminosity / renderSpacePos.MagnitudeSquared());
		LightEmissionNode.LightIntensityLux = lux > 0 ? lux : 0;
		LightEmissionNode.Rotation = SphericalCoordinates.FromCartesian(renderSpacePos).ToEuler();

		LightEmissionNodeMirror.LightIntensityLux = LightEmissionNode.LightIntensityLux;
		LightEmissionNodeMirror.Rotation = LightEmissionNode.Rotation;
	}
}
