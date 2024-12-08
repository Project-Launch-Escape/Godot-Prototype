using Godot;
using GodotPrototype.Scripts.Simulation.Physics;

namespace GodotPrototype.Scripts;

public partial class CelestialInitializer : Node
{
	private List<ConfigFile> _configs = [];
	private bool[] _createdCelestial;
	private string[] _celestialNames;
	private List<Node> _celestialNodes = [];
	
	[Export] private PackedScene _celestialPrefab;
	[Export] private PackedScene _surfacePrefab;
	[Export] private PackedScene _ringsPrefab;
	[Export] private PackedScene _lightEmitterPrefab;
	[Export] private PackedScene _surfaceGlowPrefab;
	[Export] private PackedScene _nodeTrackerPrefab;
	
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
		var celestial = (Node3D)_celestialPrefab.Instantiate();
		((CelestialScript)celestial).Mass = (double)cfg.GetValue("Properties", "Mass");
		celestial.Name = (string)cfg.GetValue("Properties", "Name");
		
		if (cfg.HasSection("Orbit")) AddOrbit(cfg,celestial);
		else celestial.Position = (Vector3)cfg.GetValue("Properties", "GalaxyPosition");
		if (cfg.HasSectionKey("Properties", "SOIRadius"))
			((CelestialScript)celestial).SOIRadius = (double)cfg.GetValue("Properties", "SOIRadius");
		if (cfg.HasSection("Surface")) AddSurface(cfg,celestial);
		if (cfg.HasSection("Rings")) AddRings(cfg,celestial);
		if (cfg.HasSection("LightEmission")) AddLightEmitter(cfg,celestial);
		if (cfg.HasSection("SurfaceGlow")) AddSurfaceGlow(cfg,celestial);
		
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
	
	private void AddOrbit(ConfigFile cfg, Node celestial)
	{
		CelestialScript parentCelestial = null;
		
		var parentName = (string)cfg.GetValue("Orbit", "ParentBody");
		
		foreach (var celestialNode in _celestialNodes)
		{
			if (celestialNode.Name == parentName)
			{
				parentCelestial = (CelestialScript)celestialNode;
			}
		}
		var a = (double)cfg.GetValue("Orbit", "SemiMajorAxis", 0d);
		var e = (double)cfg.GetValue("Orbit", "Eccentricity");
		var p = cfg.HasSectionKey("Orbit","SemiParameter") ? (double)cfg.GetValue("Orbit","SemiParameter") : a * (1 - e * e);
		if (!cfg.HasSectionKey("Orbit","SemiMajorAxis")) a = p / (1 - e * e);
		
		var w = (double)cfg.GetValue("Orbit", "ArgumentOfPeriapsis");
		var i = (double)cfg.GetValue("Orbit", "Inclination");
		var l = (double)cfg.GetValue("Orbit", "LongitudeOfAcendingNode");
		
		var n = cfg.HasSectionKey("Orbit","MeanMotion") ? (double)cfg.GetValue("Orbit", "MeanMotion") : Math.Sqrt(GlobalValues.G * parentCelestial.Mass / a) / a;
		var mAtEpoch = (double)cfg.GetValue("Orbit", "MeanAnomalyAtEpoch");
		var epoch = (double)cfg.GetValue("Orbit", "Epoch");
		
		var color = (Color)cfg.GetValue("Orbit", "Color");
		((CelestialScript)celestial).CelestialOrbit = new Orbit(p, e, w, i, l, n, mAtEpoch, epoch, parentCelestial, color);
		
		if (!cfg.HasSectionKey("Properties", "SOIRadius"))
			((CelestialScript)celestial).SOIRadius = p * Math.Pow(((CelestialScript)celestial).Mass / parentCelestial.Mass, 0.4f);
	}
	
	private void AddSurface(ConfigFile cfg, Node celestial)
	{
		var surface = _surfacePrefab.Instantiate();
		var mesh = (MeshInstance3D)surface.FindChild("Mesh");
		
		var material = new StandardMaterial3D();
		material.AlbedoTexture = (Texture2D)GD.Load((string)cfg.GetValue("Surface","SurfaceTexture"));
		
		mesh.SetMaterialOverride(material);
		celestial.AddChild(surface);
		((CelestialScript)celestial).Radius = (double)cfg.GetValue("Surface", "Radius");
	}
	
	private void AddRings(ConfigFile cfg, Node celestial)
	{
		var rings = (Sprite3D)_ringsPrefab.Instantiate();
		rings.Texture = (Texture2D)GD.Load((string)cfg.GetValue("Rings","Texture"));

		var ringRot = (Vector3)cfg.GetValue("Rings", "Axis");
		var ringScale = (float)cfg.GetValue("Rings", "Radius") / ((float)((CelestialScript)celestial).Radius * 2 * 4.5f);
		
		rings.Basis = new Basis(Quaternion.FromEuler(ringRot));
		rings.Basis = rings.Basis.Scaled(Vector3.One * ringScale);
		
		celestial.AddChild(rings);
	}
	
	private void AddLightEmitter(ConfigFile cfg, Node celestial)
	{
		if ((bool)cfg.GetValue("LightEmission", "Enabled"))
		{
			var lightEmitter = _lightEmitterPrefab.Instantiate();
			celestial.AddChild(lightEmitter);
		}
	}
	
	private void AddSurfaceGlow(ConfigFile cfg, Node celestial)
	{
		if ((bool)cfg.GetValue("SurfaceGlow", "Enabled"))
		{
			var surfaceGlow = _surfaceGlowPrefab.Instantiate();
			celestial.AddChild(surfaceGlow);
		}
	}
	
	private void AddNodeTracker(Node celestial)
	{
		var nodeTracker = _nodeTrackerPrefab.Instantiate();
		celestial.AddChild(nodeTracker);
	}
}
