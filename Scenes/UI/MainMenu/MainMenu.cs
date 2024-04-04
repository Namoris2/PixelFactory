using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class MainMenu : Control
{
	const string saveFolderPath = "user://saves/";
	public List<string> saves = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DirAccess dir = DirAccess.Open(saveFolderPath);
		if (dir == null)
		{
			GD.Print("Save folder doesn't exist. Creating new save folder");
			DirAccess.MakeDirAbsolute(saveFolderPath);
		}

		string[] files = DirAccess.GetFilesAt(saveFolderPath);
		Dictionary<string, DateTime> savesDictionary = new();

		foreach(string fileName in files)
		{
			if (fileName.GetExtension() == "save")
			{
				saves.Add(fileName);
				string savePath = FileAccess.Open(saveFolderPath + saves[^1], FileAccess.ModeFlags.Read).GetPathAbsolute();
				DateTime lastPlayed = System.IO.File.GetLastWriteTime(savePath);
				savesDictionary.Add(fileName.GetBaseName(), lastPlayed);
				//GD.Print($"Save Name: {fileName.GetBaseName()} Last Played: {lastPlayed}");
			}
		}

		//GD.Print();
		Node savesContainer = GetTree().GetNodesInGroup("LoadedSaves")[0];
		Dictionary<string, DateTime> sortedSaves = savesDictionary.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
		foreach(var save in sortedSaves)
		{
			Node loadedSave = GD.Load<PackedScene>("res://Scenes/UI/MainMenu/LoadedSave.tscn").Instantiate();
			loadedSave.GetNode<Label>("SaveName").Text = save.Key;
			loadedSave.GetNode<Label>("LastPlayed").Text = "Last Played: " + save.Value.ToString();
			savesContainer.AddChild(loadedSave);
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
