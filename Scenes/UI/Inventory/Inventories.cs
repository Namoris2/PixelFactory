using Godot;
using System;

public partial class Inventories : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		tileMap.ToggleInventory += ToggleInventory;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ToggleInventory(bool TOGGLEINGINVENTORY, string building)
	{
		if (TOGGLEINGINVENTORY)
		{
			this.Show();
		}
		else
		{
			this.Hide();
		}
	}
}
