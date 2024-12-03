using Godot;
using System;

public partial class Unpause : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PauseMenu pauseMenu = GetNode<PauseMenu>("/root/main/UI/PauseMenu");
		Pressed += pauseMenu.UnpauseGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
