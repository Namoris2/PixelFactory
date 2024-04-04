using Godot;
using System;
using System.IO;
//using System.IO;

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
		string saveName = GetNode<Label>("../SaveName").Text;
		string savePath = $"user://saves/{saveName}.save";

		Godot.FileAccess file = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Read);
		string absoluteSavePath = file.GetPathAbsolute();
		file.Close();
		File.Delete(absoluteSavePath);

		GetParent().QueueFree();
		GetNode<MainMenu>("/root/MainMenu").saves.Remove(saveName + ".save");
	}
}
