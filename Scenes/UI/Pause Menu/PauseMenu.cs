using Godot;
using System;

public partial class PauseMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Button close = GetNode<Button>("Close");
		close.Pressed += UnpauseGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		GD.Print(Visible);

		if(Input.IsActionJustPressed("Back") && !tileMap.UITOGGLE)
		{
			//UnpauseGame();
		}
	}

	private void UnpauseGame()
	{
			Visible = false;
			GetTree().Paused = false;
	}
}
