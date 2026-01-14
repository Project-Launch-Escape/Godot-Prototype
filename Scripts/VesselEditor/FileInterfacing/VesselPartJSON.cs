using System.Text.Json.Serialization;
using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.FileInterfacing;

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

	public PartComponentJSON[] Components;
	

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

		Components = new PartComponentJSON[part.Components.Length];
		for (int i = 0; i < Components.Length; i++)
		{
			Components[i] = PartComponentJSON.ComponentToJsonCompatible(part.Components[i], i);
		}
	}
	
	[JsonConstructor]
	public VesselPartJSON(int partIndex, string partDefID, int parentIndex, int parentSnapPointIndex, int snapPointIndex, Vector3 position, Vector3 rotation, Vector3 unsnappedRotation, PartComponentJSON[] components)
	{
		PartIndex = partIndex;
		PartDefID = partDefID;
		ParentIndex = parentIndex;
		ParentSnapPointIndex = parentSnapPointIndex;
		SnapPointIndex = snapPointIndex;
		Position = position;
		Rotation = rotation;
		UnsnappedRotation = unsnappedRotation;
		Components = components;
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

	public VesselPart ToPart()
	{
		var part = PartDefinition.PartDefByID[PartDefID].Scene.Instantiate<VesselPart>();

		part.PartDefID = PartDefID;
		part.Position = Position;
		part.Rotation = Rotation;
		part.UnsnappedBasis = Basis.FromEuler(UnsnappedRotation);

		for (int i = 0; i < part.Components.Length; i++)
		{
			Components[i].InitializeComponent(part.Components[i]);
		}

		return part;
	}
}
