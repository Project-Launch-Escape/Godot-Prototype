using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor;

public class VesselPartJSON
{
	public int PartIndex;
	public string PartDefID;
	public int ParentIndex; // -1 if no parent
	
	public int ParentSnapPointIndex; // Position in array of snap points on parent. -1 if surface attached, -2 if is root
	public int SnapPointIndex; // Index of snap point on this part connected to parent. -1 if surface attached, -2 if is root
	
	// uses Relative Positions and Rotations. Angles are in radians
	public Vector3 Position;
	public Vector3 Rotation; // Uses Euler angles to save Space
	public Vector3 UnsnappedRotation;

	public PartComponent[] Components;
	
	

	public VesselPartJSON(VesselPart part, int index, int parentIndex)
	{
		PartIndex = index;
		PartDefID = part.PartDef.ID;
		ParentIndex = parentIndex;

		ParentSnapPointIndex = GetIndexOfSnapPointAttachedTo(part.ParentPart, part);
		SnapPointIndex = GetIndexOfSnapPointAttachedTo(part, part.ParentPart);

		Position = part.Position;
		Rotation = part.Rotation;
		UnsnappedRotation = part.UnsnappedBasis.GetEuler();

		Components = part.Components;
	}

	private static int GetIndexOfSnapPointAttachedTo(VesselPart part, VesselPart otherPart)
	{
		var snapPointIndex = -1;
		if (part != null && otherPart != null)
		{
			var snapPoints = part.SnapPoints;
			for (int i = 0; i < snapPoints.Length; i++)
			{
				if (snapPoints[i].AttachedPart != otherPart) continue;
				snapPointIndex = i;
				break;
			}
		}
		else
		{
			snapPointIndex = -2;
		}

		return snapPointIndex;
	}
}
