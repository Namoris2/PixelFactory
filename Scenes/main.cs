using Godot;
using System;

public partial class main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().Paused = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{		
		TileMap tileMap = GetNode<TileMap>("World/TileMap");
		Control pauseMenu = GetNode<Control>("UI/PauseMenu");

		if(Input.IsActionJustPressed("Back") && !tileMap.UITOGGLE)
		{
			GetTree().Paused = !GetTree().Paused;
			pauseMenu.Visible = !pauseMenu.Visible;
		}
	}
}
