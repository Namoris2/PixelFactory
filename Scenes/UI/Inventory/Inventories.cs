using Godot;
using System;

public partial class Inventories : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ToggleInventory(bool TOGGLEINGINVENTORY)
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
