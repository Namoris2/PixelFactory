using Godot;
using Godot.Collections;
using System;
using System.Text.Json.Serialization;

public partial class SaveGame : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += Save;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Save()
	{
		FileAccess savedGame = FileAccess.Open(GetNode<main>("/root/GameInfo").savePath, FileAccess.ModeFlags.Write);
		System.Collections.Generic.Dictionary<string, dynamic> savedData = new();
		Array<Node> nodes = GetTree().GetNodesInGroup("CanSave");
		
		foreach (dynamic node in nodes)
		{
			dynamic savedNode = node.Save();
			savedData.Add(node.Name, savedNode);
		}

		if (savedGame != null)
		{
			savedGame.StoreLine(Newtonsoft.Json.JsonConvert.SerializeObject(savedData));
			GD.Print("Game Saved");
			savedGame.Close();
		}
		else
		{
			GD.Print("Saving failed");
		}
	}
}
