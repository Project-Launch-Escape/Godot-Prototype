using System.Text.Json.Serialization;
using Godot;
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
		var partDictionary = Parts.ToDictionary(part => part.PartIndex);
		var initializedPartJsons = new VesselPartJSON[Parts.Length];
		var initializedParts = new VesselPart[Parts.Length];
		
		VesselPart rootPart = null;
		
		foreach (var part in Parts)
		{
			InitializePart(part);
		}
		
		return new PartTree(rootPart);

		
		void InitializePart(VesselPartJSON partJSON)
		{
			if (initializedPartJsons.Contains(partJSON)) return;
			
			var part = partJSON.ToPart();

			if (partJSON.IsRootPart())
			{
				rootPart = part;
			}
			else
			{
				InitializePart(partDictionary[partJSON.ParentIndex]);
				if (partJSON.IsSurfaceAttached)
				{
					initializedParts[partJSON.ParentIndex].AddChild(part);
				}
				else
				{
					var snapPoint = part.SnapPoints[partJSON.SnapPointIndex]; // TODO: Handle Surface Attachment
					var parentSnapPoint = initializedParts[partJSON.ParentIndex].SnapPoints[partJSON.ParentSnapPointIndex];
				
					SnapPoint.AttachSnapPointTo(snapPoint, parentSnapPoint);
				}

				part.Position = partJSON.Position;
				part.Rotation = partJSON.Rotation;
			}
			
			
			initializedPartJsons[partJSON.PartIndex] = partJSON;
			initializedParts[partJSON.PartIndex] = part;
		}
	}
}
