using Godot;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI;

public partial class ToolSelector : ItemList
{
	[Export] private Panel _leftOutline;
	[Export] private Panel _rightOutline;
	private Vector2 _outlineSize;
	
	public override void _Ready()
	{
		_outlineSize = _leftOutline.Size;
		OnToolSelect((int)EditorTool.Place, new Vector2(), 1); // Runs selection code at startup
		OnToolSelect((int)EditorTool.Modify, new Vector2(), 2);
		
		ItemClicked += OnToolSelect;
	}

	public void OnToolSelect(long index, Vector2 clickPos, long mouseButtonIndex)
	{
		var mouseButton = (MouseButton)mouseButtonIndex;
		if (mouseButton is not MouseButton.Left and not MouseButton.Right) return;

		var selectedTool = (EditorTool)index;
		Editor.SelectTool(selectedTool, mouseButton);

		var panelToMove = mouseButton is MouseButton.Left ? _leftOutline : _rightOutline;
		const int x0 = 5,  dx = 94, y0 = 5;
		panelToMove.Position = new Vector2(x0 + index * dx, y0);

		const int expandMargins = 8;
		panelToMove.Size = _outlineSize;
		if (_leftOutline.Position == _rightOutline.Position)
		{
			panelToMove.Size += expandMargins * Vector2.One;
			panelToMove.Position -= expandMargins * Vector2.One / 2;
		}
		
		DeselectAll();
	}
}
