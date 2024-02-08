using Godot;
using System;

public partial class BuildMenu : CanvasLayer
{
	TileMap tileMap;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tileMap = GetNode<TileMap>("/root/main/World/TileMap");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ToggleBuildMode"))
		{
			ToggleBuildMode();
		}

		if (Input.IsActionJustPressed("Back") && tileMap.BUILDINGMODE)
		{
			CloseBuildingMode();
		}
	}
	public void ToggleBuildMode()
	{
		//GD.Print($"UITOGGLE: {tileMap.UITOGGLE}, buildingMenu: {HIDDEN}");
		
		if (!tileMap.UITOGGLE && !Visible)
		{
			tileMap.UITOGGLE = true;
			Visible = true;
		}
		else if (tileMap.UITOGGLE && Visible)
		{
			tileMap.UITOGGLE = false;
			Visible = false;
		}
	}
	private void CloseBuildingMode()
	{
		tileMap.UITOGGLE = false;
		Visible = false;
		tileMap.ToggleBuildMode();
	}
}
