using Godot;
using System;

public partial class BuildMenu : CanvasLayer
{
	World tileMap;
	Label worldInfo;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tileMap = GetNode<World>("/root/main/World/TileMap");
		worldInfo = GetNode<Label>("/root/main/UI/WorldInfo");
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
			worldInfo.Hide();
		}
		else if (tileMap.UITOGGLE && Visible)
		{
			tileMap.UITOGGLE = false;
			Visible = false;
			worldInfo.Show();
		}
	}
	private void CloseBuildingMode()
	{
		tileMap.UITOGGLE = false;
		Visible = false;
		tileMap.ToggleBuildMode();
		worldInfo.Visible = !Visible;
	}
}
