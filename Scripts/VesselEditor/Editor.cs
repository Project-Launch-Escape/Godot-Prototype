using Godot;
using Godot.Collections;
using GodotPrototype.Scripts.VesselEditor.EditorUI;
using GodotPrototype.Scripts.VesselEditor.EditorUI.ContextMenus;
using GodotPrototype.Scripts.Vessels;
using Array = System.Array;
using PartSelector = GodotPrototype.Scripts.VesselEditor.EditorUI.PartSelector;

namespace GodotPrototype.Scripts.VesselEditor;

public partial class Editor : Node3D
{
	public static Camera3D Camera;
	
	public static List<PackedScene> PartsScenes = [];
	
	private const string PartSceneDirectory = "res://Resources/Parts/Scenes/";
	private const string VesselFileDirectory = "res://Vessels/";
	
	public static EditorTool ToolLeft = EditorTool.Place;
	public static EditorTool ToolRight = EditorTool.Modify;
	
	//private Node3D _copiedNode; Ctrl+C functionality to be added
	
	public static Editor EditorNode;


	public override void _Ready()
	{
		InitializeParts();
		FuelType.InitializeFuelTypes();
		EditorNode = this;
		SnapPoint.SetVisualVisibility(false);
		Camera = GetViewport().GetCamera3D();
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
			case EditorTool.FuelPipe:
				throw new IndexOutOfRangeException("FuelPipe tool is not implemented");
			case EditorTool.None:
			default:
				return null;
		}
	}

	private static void InitializeParts()
	{
		var fileNames = DirAccess.GetFilesAt(PartSceneDirectory);
		foreach (var fileName in fileNames)
		{
			var filePath = PartSceneDirectory + fileName;
			var partFile = ResourceLoader.Load<PackedScene>(filePath);
			PartsScenes.Add(partFile);
		}
		PartSelector.SelectorNode.InitializeParts(PartsScenes);
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

	public static VesselPart DuplicatePart(VesselPart duplicant, bool includeDescendants = true)
	{
		duplicant.SetDescendantOwner(includeDescendants);

		var scene = new PackedScene();
		scene.Pack(duplicant);
		if (includeDescendants) duplicant.SetDescendantOwner(false);
		
		var newPart = scene.Instantiate<VesselPart>();
		//GD.Print("\n", $"OG: Mass: {duplicant.Mass}, SO: {duplicant.SnapPoints[0].Occupied}, {duplicant.ChildParts[0]}", "\n",
		//$"OG: Mass: {newPart.Mass}, SO: {newPart.SnapPoints[0].Occupied}, {newPart.ChildParts[0]}");
		return newPart;
	}

	public static void SaveVessel(VesselPart vesselRootPart)
	{
		//vesselRootPart = DuplicatePart(vesselRootPart);
		vesselRootPart.SetDescendantOwner(true);
		
		var scene = new PackedScene();
		scene.Pack(vesselRootPart);
		var error = ResourceSaver.Save(scene, VesselFileDirectory + "save.tscn");
		if (error is not Error.Ok) GD.PrintErr(error);
		
		vesselRootPart.SetDescendantOwner(false);
		//vesselRootPart.QueueFree();
	}


	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton mouseButton) HandleToolInput(mouseButton);
	}
}
