using Godot;
using System;
using System.IO;

public partial class DeleteSave : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += Delete;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void Delete()
	{
		DeleteSaveDialog deleteDialog = (DeleteSaveDialog)GetTree().GetFirstNodeInGroup("DeleteSaveDialog");
		string savePath = deleteDialog.savePath;

		Godot.FileAccess file = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Read);
		string absoluteSavePath = file.GetPathAbsolute();
		file.Close();
		File.Delete(absoluteSavePath);

		deleteDialog.Hide();
		GetNode<MainMenu>("/root/MainMenu").saves.Remove(deleteDialog.saveName + ".save");
		GetTree().GetNodesInGroup("LoadedSave")[deleteDialog.index].QueueFree();

		GD.Print("Save Deleted");
	}
}
