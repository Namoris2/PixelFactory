using Godot;
using System;
using System.Diagnostics.SymbolStore;

public partial class DrillInventory : Control
{
	[Signal]
	public delegate void DisableInventoryEventHandler();

	private bool INVENTORYOPPENED = true;
	public string INVENTORYTYPE = "machine";


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		tileMap.ToggleInventory += ToggleInventory;
		tileMap.UpdateBuildingProgress += UpdateInventory;

		//GD.Print(GetNode<Control>("Slots").GetChildCount());
		Control slots = GetNode<Control>("Slots");
		for (int i = 0; i < slots.GetChildCount(); i++)
		{
			InventorySlot slot = slots.GetChild<InventorySlot>(i);
			slot.inventoryType = "machine";

			string slotName = slot.Name;
			slotName = slotName.Replace("InputSlot", "");
			slotName = slotName.Replace("OutputSlot", "");

			slot.inventorySlotIndex = int.Parse(slotName);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ToggleInventory(bool TOGGLEINGINVENTORY, string building)
	{
		dynamic buildingInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(building);

		if (buildingInfo.type == "drill" && TOGGLEINGINVENTORY)
		{
			Label buildingName = GetNode<Label>("Name");
			Label resourceProduction = GetNode<Label>("Production");
			
			buildingName.Text = buildingInfo.name;
			resourceProduction.Text = buildingInfo.production;
			
			Vector2I coordinates = new ((int)buildingInfo.coords[0], (int)buildingInfo.coords[1]);

			Control slots = GetNode<Control>("Slots");
			for (int i = 0; i < slots.GetChildCount(); i++)
			{
				InventorySlot slot = slots.GetChild<InventorySlot>(i);
				slot.buildingCoordinates = coordinates;
				slot.itemType = buildingInfo.outputSlots[i].resource.ToString();
				slot.UpdateSlotTexture(slot.itemType);
				//GD.Print(slot.itemType);
			}

			this.Show();
		}
		else
		{
			//this.Free();
			this.Hide();
		}
	}

	private void UpdateInventory(double progress, int itemAmount, string itemName, string itemType, Vector2I coodrinates)
	{
		ProgressBar productionProgress = GetNode<ProgressBar>("ProductionProgress");
		productionProgress.Value = progress;

		Label resourceAmount = GetNode<Label>("Slots/OutputSlot0/ResourceAmount");
		InventorySlot slot = GetNode<InventorySlot>("Slots/OutputSlot0");

		if (itemAmount > 0)
		{
			if (slot.buildingCoordinates != coodrinates) { return; }
			
			resourceAmount.Text = itemAmount.ToString();
			slot.itemType = itemType;
			slot.UpdateSlotTexture(itemType);
		}
		else
		{
			resourceAmount.Text = "";
			slot.itemType = "";
			slot.UpdateSlotTexture("");
		}


		//GD.Print(coordinates.ToString());
	}
}
