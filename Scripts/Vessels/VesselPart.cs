using Godot;
using GodotPrototype.Scripts.UserInterface;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/VesselPartIcon.png")]
public partial class VesselPart : StaticBody3D, IDepictable
{
	[Export] public SnapPoint[] SnapPoints;
	[Export] public SnapPoint SurfaceAttatchPoint;

	public bool SurfaceAttatchable => SurfaceAttatchPoint != null;

	public Vessel ParentVessel;
	public VesselPart VesselRootPart => ParentVessel.RootPart;
	public VesselPart ParentPart => GetParent() as VesselPart;
	public List<VesselPart> ChildParts => GetChildParts();

	public bool IsRootPart => ParentPart == null;
	
	[Export] public PartComponent[] Components;
	public FuelSystem ConnectedFuelSystem;
	public double Mass => _baseMass + Components.Sum(component => component.Mass);
	[Export] private double _baseMass;

	public PartDefinition PartDef;
	
	[Export] public Texture2D Icon { get; set; }
	public Color IconColor { get; set; }
	[Export] public string PartName;

	public Basis UnsnappedBasis = Basis.Identity;
	
	public override void _Ready()
	{
		foreach (var component in Components)
		{
			component.ParentPart = this;
		}
	}

	public void InitializePart()
	{
		foreach (var component in Components)
		{
			switch (component)
			{
				case FuelTank tank:
					ConnectedFuelSystem.AddTank(tank);
					break;
				case RocketEngine engine:
					ParentVessel.Engines.Add(engine);
					break;
			}
		}
	}
	
	public List<VesselPart> GetAllDescendantParts()
	{
		var descendantParts = new List<VesselPart>();
		descendantParts.AddRange(ChildParts);
		
		foreach (var childPart in ChildParts)
		{ 
			descendantParts.AddRange(childPart.GetAllDescendantParts());
		}

		return descendantParts;
	}

	public List<VesselPart> GetChildParts()
	{
		var childParts = new List<VesselPart>();
		
		foreach (var child in GetChildren())
		{
			if (child is VesselPart childPart) childParts.Add(childPart);
		}
		return childParts;
	}

	public void SetDescendantOwner(bool set)
	{
		foreach (var descendant in GetAllDescendantParts())
		{
			descendant.Owner = set ? this : null;
			foreach (var component in Components)
			{
				component.Owner = set ? this : null;
			}
		}
	}
}
