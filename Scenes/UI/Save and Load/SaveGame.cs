using Godot;
using Godot.Collections;
using System;

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

	private void Save()
	{
		FileAccess savedGame = FileAccess.Open(GetNode<main>("/root/GameInfo").savePath, FileAccess.ModeFlags.Write);
		Array<Node> nodes = GetTree().GetNodesInGroup("CanSave");
		
		foreach (Node node in nodes)
		{
			string savedData = node.Call("Save").ToString();
			savedGame.StoreLine(savedData);
		}
		GD.Print("Game Saved");
		savedGame.Close();
	}
}
