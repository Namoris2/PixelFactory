using Godot;
using System;

public partial class Inventories : CanvasLayer
{
	World tileMap;
	dynamic buildingDisplayInfo;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tileMap = GetNode<World>("/root/main/World/TileMap");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
  
	public void ToggleInventory(bool toggle)
	{
		if (toggle)
		{
			this.Show();
		}
		else
		{
			this.Hide();
			GetNode<Control>("InventoryGrid/BuildingInventory").Hide();
		}
	}

	public void ToggleBuildingInventory(bool toggle, dynamic buildingData)
	{
		if (buildingData != null && (buildingData.buildingType.ToString() == "machine" || buildingData.buildingType.ToString() == "storage"))
		{
			ToggleInventory(toggle);
			GetNode<BuildingInventory>("InventoryGrid/BuildingInventory").ToggleInventory(toggle, buildingData);
		}
		else if (Visible)
		{
			ToggleInventory(false);
			GetNode<BuildingInventory>("InventoryGrid/BuildingInventory").ToggleInventory(false, "");
		}
	}
}
