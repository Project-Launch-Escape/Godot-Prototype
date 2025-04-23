using Godot;
using GodotPrototype.Scripts.Simulation;
using GodotPrototype.Scripts.Simulation.Physics;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts.Other;

public partial class CelestialInitializer : Node
{
	private List<ConfigFile> _configs = [];
	private bool[] _createdCelestial;
	private string[] _celestialNames;
	private List<Celestial> _celestialNodes = [];
	
	[Export] private PackedScene _celestialPrefab;
	[Export] private PackedScene _surfacePrefab;
	[Export] private PackedScene _ringsPrefab;
	[Export] private PackedScene _lightEmitterPrefab;
	[Export] private PackedScene _surfaceGlowPrefab;
	[Export] private PackedScene _nodeTrackerPrefab;

	public override void _EnterTree()
	{
		OrbitMesh.DefaultMeshParent = this;
	}

	public override void _Ready()
	{
		_configs = GetConfigs();
		_createdCelestial = new bool[_configs.Count];
		_celestialNames = new string[_configs.Count];

		for (int i = 0; i < _configs.Count; i++)
		{
			_celestialNames[i] = (string)_configs[i].GetValue("Properties", "Name");
		}

		for (int i = 0; i < _configs.Count; i++)
		{
			if (_createdCelestial[i]) continue;
			CreateCelestial(_configs[i]);
		}
	}

	private static List<ConfigFile> GetConfigs()
	{
		var cfgs = new List<ConfigFile>();
		var fileNames = DirAccess.GetFilesAt("res://CelestialConfigs");
		foreach (var fileName in fileNames)
		{
			var cfg = new ConfigFile();
			var filePath = "res://CelestialConfigs/" + fileName;
			cfg.Load(filePath);
			cfgs.Add(cfg);
		}
		return cfgs;
	}
	
	private void CreateCelestial(ConfigFile cfg)
	{
		if (cfg.HasSection("Orbit"))
		{
			var parentName = (string)cfg.GetValue("Orbit", "ParentBody");
			
			for (int i = 0; i < _configs.Count; i++)
			{
				if (_celestialNames[i] != parentName) continue;
				if (!_createdCelestial[i])
				{
					CreateCelestial(_configs[i]);
				}
				break;
			}
		}
		var celestial = (Celestial)_celestialPrefab.Instantiate();
		celestial.Mass = (double)cfg.GetValue("Properties", "Mass");
		celestial.Name = (string)cfg.GetValue("Properties", "Name");
		
		if (cfg.HasSection("Orbit")) AddOrbit(cfg,celestial);
		else celestial.Position = (Vector3)cfg.GetValue("Properties", "GalaxyPosition");
		if (cfg.HasSectionKey("Properties", "SOIRadius"))
			celestial.SOIRadius = (double)cfg.GetValue("Properties", "SOIRadius");
		if (cfg.HasSection("Surface")) AddSurface(cfg, celestial);
		if (cfg.HasSection("Rings")) AddRings(cfg, celestial);
		if (cfg.HasSection("LightEmission")) AddLightEmitter(cfg, celestial);
		
		_celestialNodes.Add(celestial);
		AddChild(celestial);
		if (cfg.HasSection("Properties")) AddNodeTracker(celestial);
		
		for (int i = 0; i < _configs.Count; i++)
		{
			if (_celestialNames[i] != celestial.Name) continue;
			_createdCelestial[i] = true;
			break;
		}
	}
	
	private void AddOrbit(ConfigFile cfg, Celestial celestial)
	{
		Celestial parentCelestial = null;
		
		var parentName = (string)cfg.GetValue("Orbit", "ParentBody");
		
		foreach (var celestialNode in _celestialNodes)
		{
			if (celestialNode.Name != parentName) continue;
			celestialNode.ChildCelestials.Add(celestial);
			parentCelestial = celestialNode;
		}
		
		var a = (double)cfg.GetValue("Orbit", "SemiMajorAxis", 0d);
		var e = (double)cfg.GetValue("Orbit", "Eccentricity");
		var p = cfg.HasSectionKey("Orbit","SemiParameter") ? (double)cfg.GetValue("Orbit","SemiParameter") : a * (1 - e * e);
		if (!cfg.HasSectionKey("Orbit","SemiMajorAxis")) a = p / (1 - e * e);
		
		var w = (double)cfg.GetValue("Orbit", "ArgumentOfPeriapsis");
		var i = (double)cfg.GetValue("Orbit", "Inclination");
		var l = (double)cfg.GetValue("Orbit", "LongitudeOfAcendingNode");
		
		var n = cfg.HasSectionKey("Orbit","MeanMotion") ? (double)cfg.GetValue("Orbit", "MeanMotion") : Math.Sqrt(GlobalValues.G * parentCelestial.Mass / a) / a;

		var tpp = cfg.HasSectionKey("Orbit", "TimeOfPeriapsisPassage") ? (double)cfg.GetValue("Orbit", "TimeOfPeriapsisPassage")
			: (double)cfg.GetValue("Orbit", "Epoch") - (double)cfg.GetValue("Orbit", "MeanAnomalyAtEpoch") / n;
		
		var color = (Color)cfg.GetValue("Orbit", "Color");
		celestial.CelestialOrbit = new Orbit(p, e, w, i, l, n, tpp, parentCelestial, color);
		
		if (!cfg.HasSectionKey("Properties", "SOIRadius"))
			celestial.SOIRadius = p * Math.Pow(celestial.Mass / parentCelestial.Mass, 0.4f);
	}
	
	private void AddSurface(ConfigFile cfg, Celestial celestial)
	{
		var surface = (MeshInstance3D)_surfacePrefab.Instantiate();
		
		var material = new StandardMaterial3D();
		material.AlbedoTexture = (Texture2D)GD.Load((string)cfg.GetValue("Surface","SurfaceTexture"));
		var radius = (double)cfg.GetValue("Surface", "Radius");
		celestial.Radius = radius;
		celestial.SurfaceNode = surface;
		
		surface.Scale = Vector3.One * (float)radius * 2;
		surface.SetMaterialOverride(material);
		celestial.AddChild(surface);
		if (cfg.HasSection("SurfaceGlow")) AddSurfaceGlow(cfg, surface);
	}
	
	private void AddRings(ConfigFile cfg, Celestial celestial)
	{
		var rings = (Sprite3D)_ringsPrefab.Instantiate();
		rings.Texture = (Texture2D)GD.Load((string)cfg.GetValue("Rings","Texture"));

		var ringRot = (Vector3)cfg.GetValue("Rings", "Axis");
		var ringScale = (float)cfg.GetValue("Rings", "Radius") / 4.5f;
		
		rings.Basis = new Basis(Quaternion.FromEuler(ringRot));
		rings.Basis = rings.Basis.Scaled(Vector3.One * ringScale);
		
		celestial.AddChild(rings);
	}
	
	private void AddLightEmitter(ConfigFile cfg, Celestial celestial)
	{
		if (!(bool)cfg.GetValue("LightEmission", "Enabled")) return;
		
		var lightEmitter = (DirectionalLight3D)_lightEmitterPrefab.Instantiate();
		celestial.AddChild(lightEmitter);
		celestial.LightEmissionNode = lightEmitter;
		
		var lightEmitterMirror = (DirectionalLight3D)_lightEmitterPrefab.Instantiate();
		GlobalValues.LocalSpaceCamera.GetParent().AddChild(lightEmitterMirror);
		celestial.LightEmissionNodeMirror = lightEmitterMirror;
		
		celestial.Luminosity = (double)cfg.GetValue("LightEmission", "Luminosity");
	}
	
	private void AddSurfaceGlow(ConfigFile cfg, Node surface)
	{
		if ((bool)cfg.GetValue("SurfaceGlow", "Enabled"))
		{
			var surfaceGlow = _surfaceGlowPrefab.Instantiate();
			surface.AddChild(surfaceGlow);
		}
	}
	
	private void AddNodeTracker(Celestial celestial)
	{
		var nodeTracker = _nodeTrackerPrefab.Instantiate();
		celestial.AddChild(nodeTracker);
	}
}
