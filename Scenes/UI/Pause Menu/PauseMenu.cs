using Godot;
using System;

public partial class PauseMenu : Control
{
	public bool CanPause = true;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().Paused = true;

		Button close = GetNode<Button>("Close");
		close.Pressed += UnpauseGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		//GD.Print($"UI: {tileMap.UITOGGLE} BUILDINGMODE: {tileMap.BUILDINGMODE}");

		if(Input.IsActionJustPressed("Back") && CanPause)
		{
			Visible = !Visible;
			GetTree().Paused = !GetTree().Paused;
		}
	}

	private void UnpauseGame()
	{
			Visible = false;
			GetTree().Paused = false;
	}
}
