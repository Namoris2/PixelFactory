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
		DeleteSaveDialog parent = (DeleteSaveDialog)GetParent();
		string savePath = parent.savePath;

		Godot.FileAccess file = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Read);
		string absoluteSavePath = file.GetPathAbsolute();
		file.Close();
		File.Delete(absoluteSavePath);

		parent.Hide();
		GetNode<MainMenu>("/root/MainMenu").saves.Remove(parent.saveName + ".save");
		GetTree().GetNodesInGroup("LoadedSave")[parent.index].QueueFree();

		GD.Print(parent.index);
		GD.Print("Save Deleted");
	}
}
