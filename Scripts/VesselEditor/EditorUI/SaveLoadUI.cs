using Godot;
using GodotPrototype.Scripts.VesselEditor.FileInterfacing;

namespace GodotPrototype.Scripts.VesselEditor.EditorUI;

public partial class SaveLoadUI : Node
{
	[Export] private FileDialog _fileWindow;
	[Export] public MenuBar ActionBar;
	[Export] private Label _currentFilelabel;
	
	private string _currentFilePath;
	public string CurentFilePath
	{
		set
		{
			_currentFilelabel.Text = GetPrettyFilePath(value);
			_currentFilePath = value;
		}
		get => _currentFilePath;
	}

	public override void _Ready()
	{
		var filePopup = ActionBar.GetMenuPopup(0);
		filePopup.IdPressed += OnFileItemClicked;
		_fileWindow.FileSelected += OnFileSelect;
	}

	private void OnFileItemClicked(long id)
	{
		switch (id)
		{
			case 0:
				OnSaveClicked();
				break;
			case 1:
				OnSaveAsClicked();
				break;
			case 2:
				OnLoadClicked();
				break;
			case 3:
				OnNewClicked();
				break;
			default:
				return;
		}
	}

	private string GetPrettyFilePath(string filePath)
	{
		int slashIndex = 0;
		for (int i = 0; i < filePath.Length - 1; i++)
		{
			if (filePath[i] == '/') slashIndex = i + 1;
		}

		return filePath[slashIndex..];
	}

	private void OnFileSelect(string filePath)
	{
		CurentFilePath = filePath;
		switch (_fileWindow.FileMode)
		{
			case FileDialog.FileModeEnum.SaveFile:
				VesselFileTools.SaveEditorStateToFile(filePath);
				break;
			case FileDialog.FileModeEnum.OpenFile:
				VesselFileTools.LoadEditorFileFromFilePath(filePath);
				break;
		}
	}

	private void OnSaveClicked()
	{
		if (string.IsNullOrEmpty(CurentFilePath)) return;
		VesselFileTools.SaveEditorStateToFile(CurentFilePath);
	}

	private void OnSaveAsClicked()
	{
		_fileWindow.FileMode = FileDialog.FileModeEnum.SaveFile;
		_fileWindow.Visible = true;
	}

	private void OnLoadClicked()
	{
		_fileWindow.FileMode = FileDialog.FileModeEnum.OpenFile;
		_fileWindow.Visible = true;
	}

	private void OnNewClicked()
	{
		CurentFilePath = "";
		Editor.EditorNode.ClearEditor();
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		switch (inputEvent)
		{
			case InputEventKey {Keycode: Key.S, CtrlPressed: true, ShiftPressed: false}:
				OnSaveClicked();
				break;
			case InputEventKey {Keycode: Key.S, CtrlPressed: true, ShiftPressed: true}:
				OnSaveAsClicked();
				break;
		}
	}
}
