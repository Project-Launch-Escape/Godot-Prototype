using Godot;

namespace GodotPrototype.Scripts.Vessels;

public class PartTree
{
    public VesselPart RootPart;
    public readonly List<VesselPart> Parts;
    public List<RocketEngine> Engines = [];

    public Vessel ParentVessel;
    public bool HasParentVessel => ParentVessel != null;

    public PartTree()
    {
        Parts = [];
    }

    public PartTree(VesselPart root, Vessel vessel)
    {
        RootPart = root;
        ParentVessel = vessel;
        
        Parts = [root];
        Parts.AddRange(root.GetAllDescendantParts());
    }

    public void InitializeParts()
    {
        foreach (var part in Parts)
        {
            part.InitializePart();
            if (!HasParentVessel) continue;
            part.ParentVessel = ParentVessel;
            part.ConnectedFuelSystem = ParentVessel.FuelSystems[0];
        }
    }
    
    public double GetTotalMass() => Parts.Sum(part => part.Mass);
}