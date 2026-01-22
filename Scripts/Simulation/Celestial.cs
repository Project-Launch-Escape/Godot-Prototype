using Godot;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.Simulation.DoublePrecision;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.Simulation.ReferenceFrames;
using GodotPrototype.Scripts.UserInterface;
using GodotPrototype.Scripts.UserInterface.UIElements;
using GodotPrototype.Scripts.UserInterface.UIElements.OrbitMarkers;

namespace GodotPrototype.Scripts.Simulation;

[GlobalClass, Icon("res://Resources/Icons/CelestialIcon.png")]
public partial class Celestial : Node3D, IRenderable, IOrbiter, IDepictable
{
	public static readonly List<Celestial> AllCelestials = [];
	public static readonly Dictionary<string, Celestial> CelestialDict = [];
	
	public double Mass;
	public double Radius;
	public double SOIRadius;

	public double Mu => Mass * GlobalValues.G;
	
	public Orbit CelestialOrbit;
	public RelativePosition PositionRel = new ();
	public RelativeVelocity VelocityRel = new ();
	public Celestial ParentCelestial => CelestialOrbit?.Primary;
	
	public readonly List<Celestial> ChildCelestials = [];
	public MeshInstance3D SurfaceNode;
	
	public DirectionalLight3D LightEmissionNode;
	public DirectionalLight3D LightEmissionNodeMirror; //Same as above but in LocalSpace scene
	public double Luminosity;

	public Texture2D Icon { get; set; }
	public Color IconColor { get; set; }


	public override void _Ready()
	{
		CelestialOrbit.CreateOrbitLine();
		if (!CelestialOrbit.IsStatic)
		{
			PositionRel = new RelativePosition(CelestialOrbit.PositionCurrent(), ParentCelestial);
			VelocityRel = new RelativeVelocity(CelestialOrbit.VelocityCurrent(), ParentCelestial);
			

			CelestialOrbit.CreateMarkerOfType(OrbitMarkerType.Apoapsis);
			CelestialOrbit.CreateMarkerOfType(OrbitMarkerType.Periapsis);
		}
		else
		{
			PositionRel = new RelativePosition(Transform.Origin);
			VelocityRel = new RelativeVelocity();
		}
		
		CelestialOrbit.CreateMarkerOfType(OrbitMarkerType.Position);
		
		AllCelestials.Add(this);
		CelestialDict.Add(Name, this);
		FlightCamera.AddToRenderSpaceUpdate(this);
	}
	
	public override void _Process(double dt)
	{
		if (GlobalValues.Paused) return;
		if (!CelestialOrbit.IsStatic)
		{
			PositionRel.LocalPosition = CelestialOrbit.PositionCurrent();
			VelocityRel.LocalVelocity = CelestialOrbit.VelocityCurrent();
		}
	}

	public Vector3d GetSurfacePosition(double latitude, double longitude) => new SphericalCoordinates(Radius, longitude, latitude - Math.PI / 2).ToCartesian();

	public void RenderUpdate()
	{
		var renderSpacePos = PositionRel[CoordinateSpace.RenderSpace];
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
