using Godot;
using System;

public partial class DeleteSaveDialog : Panel
{
	public string savePath;
	public string saveName;
	public int index;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("Cancel").Pressed += Hide;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
