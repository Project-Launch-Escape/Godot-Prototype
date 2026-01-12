using Godot;
using GodotPrototype.Scripts.Vessels;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI;

public partial class PartSelector : ItemList
{
	public static PartSelector SelectorNode;
	[Export] private Color _itemBackgroundColor = new (0.5f,0.5f,0.5f,0.2f);
	
	public override void _Ready()
	{
		if (SelectorNode != null) GD.PrintErr("Multiple PartSelector Nodes!");
		SelectorNode = this;
		ItemClicked += OnItemClicked;
	}
	
	public void InitializeParts(List<PartDefinition> partDefinitions)
	{
		foreach (var partDef in partDefinitions)
		{
			AddPartToUI(partDef);
		}

		for (int i = 0; i < ItemCount; i++)
		{
			SetItemCustomBgColor(i, _itemBackgroundColor);
		}
	}

	private void AddPartToUI(PartDefinition partDef)
	{
		AddItem(partDef.Name, partDef.Icon);
	}

	public void OnItemClicked(long index, Vector2 clickPos, long mouseButtonIndex)
	{
		bool correctMouse = Editor.ToolLeft is EditorTool.Place && mouseButtonIndex == 1;
		if (!correctMouse)
		{
			correctMouse = Editor.ToolRight is EditorTool.Place && mouseButtonIndex == 2;
		}
		
		if (correctMouse)
		{
			PlaceTool.ToolNode.SelectPart((int)index);
		}
		DeselectAll();
	}
}
