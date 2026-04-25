using Godot;
using Godot.Collections;
using GodotPrototype.Scripts.Other;
using GodotPrototype.Scripts.VesselEditor;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.UserInterface;

public partial class PartHover : Node
{
	public static Camera3D Camera => GlobalValues.CurrentScene is SceneType.VesselEditor ? Editor.Camera : GlobalValues.LocalSpaceCamera;
	[Export] private ShaderMaterial _outlineShader;
	private List<VesselPart> _hoveredParts = [];
	
	public override void _Process(double delta)
	{
		var hoveredPart = GetMouseHoveredPart();
		var currentHoveredParts = new List<VesselPart>();
		if (hoveredPart != null)
		{
			currentHoveredParts.Add(hoveredPart);
			if (GlobalValues.CurrentScene is SceneType.VesselEditor) 
				currentHoveredParts.AddRange(hoveredPart.GetAllDescendantParts());
		}
		
		foreach (var currentHoveredPart in currentHoveredParts)
		{
			if (_hoveredParts.Contains(currentHoveredPart)) continue;
			currentHoveredPart.Mesh.GetActiveMaterial(0).NextPass = _outlineShader;
			_hoveredParts.Add(currentHoveredPart);
			currentHoveredPart.TreeExiting += () => _hoveredParts.Remove(currentHoveredPart);
		}
		foreach (var previousHoveredPart in _hoveredParts.ToList())
		{
			if (currentHoveredParts.Contains(previousHoveredPart)) continue;
			previousHoveredPart.Mesh.GetActiveMaterial(0).NextPass = null;
			_hoveredParts.Remove(previousHoveredPart);
		}
	}
	public static Dictionary GetMouseHoveredRayResults(float distance = 100f, Array<Rid> excludeList = null)
	{
		var cameraPos = Camera.GlobalPosition;
		var rayDirection = Camera.ProjectRayNormal(Camera.GetViewport().GetMousePosition());

		var physicsSpaceState = Camera.GetWorld3D().DirectSpaceState;
		
		excludeList ??= [];

		var rayParameters = new PhysicsRayQueryParameters3D
		{
			From = cameraPos,
			To = cameraPos + rayDirection * distance,
			CollisionMask = ColMask.Parts,
			Exclude = excludeList
		};
		return physicsSpaceState.IntersectRay(rayParameters);
	}
	public static VesselPart GetMouseHoveredPart(float distance = 100f, Array<Rid> excludeList = null)
	{
		var rayResults = GetMouseHoveredRayResults(distance, excludeList);
		if (rayResults.Count == 0) return null;
		var hoveredPart = (VesselPart)rayResults["collider"];
		return hoveredPart;
	}
}
