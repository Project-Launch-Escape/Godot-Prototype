using System.Text.Json.Serialization;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public class PartTreeJSON
{
    public VesselPartJSON[] Parts;


    public PartTreeJSON(VesselPart root)
    {
        List<VesselPart> vesselParts = [root];
        vesselParts.AddRange(root.GetAllDescendantParts());

        Parts = new VesselPartJSON[vesselParts.Count];

        for(int i = 0; i < vesselParts.Count; i++)
        {
            var part = vesselParts[i];
            int parentIndex = vesselParts.IndexOf(part.ParentPart);
            Parts[i] = new VesselPartJSON(part, i, parentIndex);
        }
        
    }
    
    [JsonConstructor]
    public PartTreeJSON(VesselPartJSON[] parts)
    {
        Parts = parts;
    }

    public PartTree ToPartTree()
    {
        throw new NotImplementedException();
    }
}