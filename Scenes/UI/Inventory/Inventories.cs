using Godot;
using System;
using System.Collections.Generic;

public partial class Inventories : CanvasLayer
{
	World tileMap;
	HoldingItem holdingItem;
	PlayerInventory playerInventory;
	Label worldInfo;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tileMap = GetNode<World>("/root/main/World/TileMap");
		holdingItem = GetNode<HoldingItem>("HoldingItem");
		playerInventory = GetNode<PlayerInventory>("InventoryGrid/PlayerInventory");
		worldInfo = GetNode<Label>("../WorldInfo");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
  
	public void ToggleInventory(bool toggle, bool showCraftingMenu = true)
	{
		tileMap.UITOGGLE = toggle;
		worldInfo.Visible = !toggle;
		GameEvents.camera.toggleZoom = !toggle;
		
		if (toggle)
		{
			if (showCraftingMenu)
			{
				CraftingMenu craftingMenu = GetNode<CraftingMenu>("InventoryGrid/CraftingMenu");
				craftingMenu.ChangeRecipe(craftingMenu.selectedRecipe);
				craftingMenu.Show();
			}
			GameEvents.closePopup.Show();
			GameEvents.harvestResourcePopup.Hide();
			GameEvents.rotatePopup.Hide();
			GameEvents.toggleInventoryPopup.SetCustomActionText();
			GameEvents.toggleBuildingInventoryPopup.Hide();
			GameEvents.toggleBuildMenuPopup.Hide();
			GameEvents.toggleDismantleModePopup.Hide();
			GameEvents.toggleResearchMenuPopup.Hide();
			if (IsInstanceValid(GameEvents.tutorialContainer)) { GameEvents.tutorialContainer.Hide(); }
			Show();
		}
		else
		{
			Hide();
			if (!(tileMap.BUILDINGMODE || tileMap.DISMANTLEMODE)) { GameEvents.closePopup.Hide(); }
			GameEvents.pickUpItemPopup.Hide();
			if (tileMap.BUILDINGMODE && (bool)tileMap.buildings[tileMap.selectedBuilding].canRotate) { GameEvents.rotatePopup.Show(); }
			GameEvents.toggleInventoryPopup.SetDefaultActionText();
			GameEvents.toggleBuildMenuPopup.Show();
			GameEvents.toggleDismantleModePopup.Show();
			GameEvents.toggleResearchMenuPopup.Show();
			if (IsInstanceValid(GameEvents.tutorialContainer)) { GameEvents.tutorialContainer.Show(); }

			GetNode<Control>("InventoryGrid/BuildingInventory").Hide();
			GetNode<Control>("InventoryGrid/CraftingMenu").Hide();

			if (holdingItem.ISHOLDINGITEM)
			{
				int amount = playerInventory.PutToInventory(holdingItem.itemName, int.Parse(holdingItem.itemAmount));
				if (amount != 0)
				{
					Dictionary<string, int> item = new(){ { holdingItem.itemName, int.Parse(holdingItem.itemAmount) } };
					tileMap.CreateRemainsBox(item);
				}
				
				holdingItem.HideHoldingItem();
			}
		}
	}

	public void ToggleBuildingInventory(bool toggle, dynamic buildingData, Leftovers leftovers = null)
	{
		// GD.Print("RemainsBox null: ", remainsBox == null);
		if (leftovers != null || (buildingData != null && (buildingData.buildingType.ToString() == "machine" || buildingData.buildingType.ToString() == "storage")))
		{
			ToggleInventory(toggle, false);
			GetNode<BuildingInventory>("InventoryGrid/BuildingInventory").ToggleInventory(toggle, buildingData, leftovers);
		}
		else if (Visible)
		{
			ToggleInventory(false);
			GetNode<BuildingInventory>("InventoryGrid/BuildingInventory").ToggleInventory(false, "");
		}
	}
}
