using Godot;
using GodotPrototype.Scripts.VesselEditor;
using GodotPrototype.Scripts.VesselEditor.EditorUI;

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

    public PartTree(VesselPart root)
    {
        RootPart = root;
        Parts = [root];
        Parts.AddRange(root.GetAllDescendantParts());
    }

    public PartTree(VesselPart rootPart, List<VesselPart> parts)
    {
        RootPart = rootPart;
        Parts = parts;
    }

    public PartTree(VesselPart root, Vessel vessel) : this(root)
    {
        ParentVessel = vessel;
    }

    public void InitializeParts()
    {
        foreach (var part in Parts)
        {
            if (!HasParentVessel) continue;
            part.ParentTree = this;
            part.ConnectedFuelSystem = ParentVessel.FuelSystems[0];
            part.InitializePart();
        }
    }

    public void Reroot(VesselPart newRoot)
    {
        if (newRoot.IsSurfaceAttached)
        {
            MouseAlertHandler.CreateMouseAlert("New Root cannot be Surface Attached!", 5);
            return;
        }
        
        RootPart = newRoot;
        foreach (var part in Parts)
        {
            if (part.IsSurfaceAttached) continue;
            part.Reparent(PlaceTool.ToolNode);
        }

        List<VesselPart> rerootedParts = [newRoot];
        
        AddPartToTree(newRoot);

        return;
        void AddPartToTree(VesselPart party)
        {
            foreach (var snapPoint in party.SnapPoints)
            {
                if (!snapPoint.Occupied) continue;
                var attachedPart = snapPoint.AttachedSnapPoint.ParentPart;
                if (rerootedParts.Contains(attachedPart)) continue;
                //TODO: add reversing of surface attachments
                
                attachedPart.Reparent(party);
                rerootedParts.Add(attachedPart);
                AddPartToTree(attachedPart);
            }
        }
    }

    public void AppendTree(PartTree otherTree)
    {
        Parts.AddRange(otherTree.Parts);
        foreach (var childTreePart in otherTree.Parts)
        {
            childTreePart.ParentTree = this;
        }
    }

    public void SplitTree(VesselPart newRoot)
    {
        List<VesselPart> newTreeParts = [newRoot];
        newTreeParts.AddRange(newRoot.GetAllDescendantParts());

        var newTree = new PartTree(newRoot, newTreeParts);
        
        foreach (var partToRemove in newTreeParts)
        {
            Parts.Remove(partToRemove);
            partToRemove.ParentTree = newTree;
        }
    }
    
    
    public double GetTotalMass() => Parts.Sum(part => part.Mass);

    public override string ToString()
    {
        string str = "Tree with Root [" + RootPart.Name+ "] has " + Parts.Count + " parts, including:\n";
        foreach (var part in Parts)
        {
            str += part.Name + "\n";
        }

        return str;
    }
}