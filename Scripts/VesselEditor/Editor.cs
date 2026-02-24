using Godot;
using Godot.Collections;
using GodotPrototype.Scripts.VesselEditor.EditorUI;
using GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;
using GodotPrototype.Scripts.VesselEditor.FileInterfacing;
using GodotPrototype.Scripts.Vessels;
using Array = System.Array;
using PartSelector = GodotPrototype.Scripts.VesselEditor.EditorUI.PartSelector;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class Editor : Node3D
{
	public static VesselEditorCamera Camera;
	
	public const string PartSceneDirectory = "res://Resources/Parts/Scenes/";
	public const string PartDefinitionDirectory = "res://Resources/Parts/Definitions/";
	public const string VesselFileDirectory = "res://Saves/Vessels/";
	public const string SavesDirectory = "res://Saves/";
	
	public static EditorTool ToolLeft = EditorTool.Place;
	public static EditorTool ToolRight = EditorTool.Modify;
	
	public static Editor EditorNode;
	
	[Export] private ShaderMaterial _outlineShader;
	private List<VesselPart> _hoveredParts = [];

	public override void _Ready()
	{
		InitializeParts();
		FuelType.InitializeFuelTypes();
		EditorNode = this;
		SnapPoint.SetGlobalVisibility(false);
		Camera = (VesselEditorCamera)GetViewport().GetCamera3D();
	}

	public static bool IsToolEnabled(EditorTool tool) => tool == ToolLeft || tool == ToolRight;
	public static bool IsToolActive(EditorTool tool) => ToolObjFromEnum(tool).IsToolActive;

	public static IToolable ToolObjFromEnum(EditorTool tool)
	{
		switch (tool)
		{
			case EditorTool.Place:
				return PlaceTool.ToolNode;
			case EditorTool.Transform:
				return TranslateTool.ToolNode;
			case EditorTool.Modify:
				return ContextMenuController.ControllerNode;
			case EditorTool.Attach:
				return AttachTool.ToolNode;
			case EditorTool.Reroot:
				return RerootTool.ToolNode;
			case EditorTool.FuelPipe:
				throw new IndexOutOfRangeException("FuelPipe tool is not implemented");
			case EditorTool.None:
			default:
				return null;
		}
	}

	public static void InitializeParts()
	{
		var fileNames = DirAccess.GetFilesAt(PartDefinitionDirectory);
		foreach (var fileName in fileNames)
		{
			var filePath = PartDefinitionDirectory + fileName;
			var partFile = ResourceLoader.Load<PartDefinition>(filePath);
			PartDefinition.AddDefinition(partFile);
		}
		PartSelector.SelectorNode?.InitializeParts(PartDefinition.PartDefinitions);
	}
	
	public VesselPart[] GetRootParts()
	{
		var rootParts = new List<VesselPart>();

		foreach (var child in PlaceTool.ToolNode.GetChildren())
		{
			if (child is VesselPart childPart) rootParts.Add(childPart);
		}

		return rootParts.ToArray();
	}

	private static void HandleToolInput(InputEventMouseButton mouseInput)
	{
		var tool = mouseInput.ButtonIndex switch
		{
			MouseButton.Left => ToolLeft,
			MouseButton.Right => ToolRight,
			_ => EditorTool.None
		};
		if (tool is EditorTool.None) return;
		
		ToolObjFromEnum(tool).HandleTool(mouseInput, Input.IsKeyPressed(Key.Shift), Input.IsKeyPressed(Key.Alt));
	}

	public override void _Process(double delta)
	{
		var hoveredPart = GetMouseHoveredPart();
		var currentHoveredParts = new List<VesselPart>();
		if (hoveredPart != null)
		{
			currentHoveredParts.Add(hoveredPart);
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

	public static void SelectTool(EditorTool tool, MouseButton mouseButton)
	{
		bool isLeft = mouseButton is MouseButton.Left;
		var toolPrev = isLeft ? ToolLeft : ToolRight;

		if (toolPrev != tool) // If we are selecting a new tool. Will not run OnToolEnabled if tool was already enabled
		{
			if (ToolLeft != ToolRight) ToolObjFromEnum(toolPrev).OnToolDisable(); //Conditions: toolPrev != tool, toolPrev is not double selected
			if (!IsToolEnabled(tool)) ToolObjFromEnum(tool).OnToolEnable(); // Conditions: toolPrev != tool, tool is currently disabled
		}
			
		if (isLeft) ToolLeft = tool;
		else ToolRight = tool;
	}

	public static VesselPart DuplicatePart(VesselPart duplicant)
	{
		var jsonString = VesselFileTools.GetPartTreeJsonString(duplicant);
		return VesselFileTools.GetVesselRootFromJson(jsonString);
	}

	public void ClearEditor()
	{
		foreach (var root in GetRootParts())
		{
			root.QueueFree();
		}
	}


	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton mouseButton) HandleToolInput(mouseButton);
	}
}
