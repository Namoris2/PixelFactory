using Godot;
using System;

public partial class ExitGame : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += QuitGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void QuitGame()
	{
		GetTree().Quit();
	}
}
