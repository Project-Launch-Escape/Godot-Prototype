namespace GodotPrototype.Scripts.Vessels;

public partial class Decoupler : PartComponent
{
    public SnapPoint[] ToDetach;

    public void Decouple()
    {
        foreach (var snapPoint in ToDetach)
        {
            // 1) Get a list of the decoupled parts (parts lower on the tree than the snap points)
            // 2) Remove these parts from the root tree
            // 3) Create a new tree and vessel containing the decoupled parts
            var decoupledParts = snapPoint.AttachedPart?.GetAllDescendantParts();
            if (decoupledParts == null) continue;
            
            var newTree = ParentTree.SplitTree(snapPoint.AttachedPart);
            var newPosition = ParentVessel.PositionLocal + GlobalPosition - ParentVessel.GlobalPosition;
            var decoupledVessel = Vessel.CreateVessel(newTree, ParentVessel.ParentBody, newPosition, ParentVessel.VelocityLocal);
            Vessel.VesselParent.AddChild(decoupledVessel);
        }
    }
}