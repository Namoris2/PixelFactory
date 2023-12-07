using Godot;
using System;

public partial class BuildMenu : CanvasLayer
{
	public bool HIDDEN = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ToggleBuildMode"))
		{
			ToggleBuildMode();
		}

		if (Input.IsActionJustPressed("Back") && !HIDDEN)
		{
			ToggleBuildMode();
		}
	}
	public void ToggleBuildMode()
	{
		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		//GD.Print($"UITOGGLE: {tileMap.UITOGGLE}, buildingMenu: {HIDDEN}");
		
		if (!tileMap.UITOGGLE && HIDDEN)
		{
			tileMap.UITOGGLE = true;
			HIDDEN = false;
			this.Show();
		}
		else if (tileMap.UITOGGLE && !HIDDEN)
		{
			tileMap.UITOGGLE = false;
			HIDDEN = true;
			this.Hide();

			tileMap.BUILDINGMODE = true;
			tileMap.ToggleBuildMode();
		}
	}
}
