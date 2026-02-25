using System.Text.Json.Serialization;
using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

public class PartTreeJSON
{
	public VesselPartJSON[] Parts;


	public PartTreeJSON(PartTree partTree)
	{
		Parts = new VesselPartJSON[partTree.Parts.Count];

		for(int i = 0; i < partTree.Parts.Count; i++)
		{
			var part = partTree.Parts[i];
			int parentIndex = partTree.Parts.IndexOf(part.ParentPart);
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
					var snapPoint = part.SnapPoints[partJSON.SnapPointIndex];
					var parentSnapPoint = initializedParts[partJSON.ParentIndex].SnapPoints[partJSON.ParentSnapPointIndex];
				
					SnapPoint.AttachSnapPoints(snapPoint, parentSnapPoint);
				}

				part.Position = partJSON.Position;
				part.Rotation = partJSON.Rotation;
			}
			
			
			initializedPartJsons[partJSON.PartIndex] = partJSON;
			initializedParts[partJSON.PartIndex] = part;
		}
	}
}
