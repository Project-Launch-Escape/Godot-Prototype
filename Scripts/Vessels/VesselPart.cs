using Godot;

namespace GodotPrototype.Scripts.Vessels;

[GlobalClass, Icon("res://Resources/Icons/VesselPartIcon.png")]
public partial class VesselPart : StaticBody3D
{
	[Export] public SnapPoint[] SnapPoints;
	[Export] public SnapPoint SurfaceAttatchPoint;

	public bool SurfaceAttatchable => SurfaceAttatchPoint != null;

	public Vessel ParentVessel;
	public VesselPart VesselRootPart => ParentVessel.RootPart;
	public VesselPart ParentPart => GetParent() as VesselPart;
	public List<VesselPart> ChildParts = [];

	public bool IsRootPart;
	
	[Export] public PartComponent[] Components;
	public FuelSystem ConnectedFuelSystem;
	public double Mass => _baseMass + Components.Sum(component => component.Mass);
	[Export] private double _baseMass;

	
	public override void _Ready()
	{
		foreach (var component in Components)
		{
			component.ParentPart = this;
		}

		var nodeChildren = GetChildren();
		foreach (var child in nodeChildren)
		{
			if (child is VesselPart childPart) ChildParts.Add(childPart);
		}
	}

	public void InitializePart()
	{
		foreach (var component in Components)
		{
			switch (component)
			{
				case FuelTank tank:
					ConnectedFuelSystem.ConnectedTanks.Add(tank);
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
}
