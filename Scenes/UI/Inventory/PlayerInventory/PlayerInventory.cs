using Godot;
using System;
using System.Diagnostics.SymbolStore;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

public partial class PlayerInventory : Control
{
	FlowContainer flowContainer;
	private InventorySlot inventorySlot;
	private string inventorySlotPath = "res://Scenes/UI/Inventory/InventorySlot.tscn";
	public InventorySlot[] inventorySlots;
	dynamic items;
	
	[Export]
	private int inventorySize = 0; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		inventorySlots = new InventorySlot[inventorySize];

		flowContainer = GetNode<FlowContainer>("FlowContainer");
		CreateInventorySlots();
		items = inventorySlots[0].items;

		// cheat items to inventory
		PutToInventory("IronIngot", 200);
		PutToInventory("CopperIngot", 200);
		PutToInventory("IronPlate", 400);
		PutToInventory("IronRod", 400);
		PutToInventory("Wire", 500);
	}

	private void CreateInventorySlots()
	{
		for (int i = 0; i < inventorySize; i++)
		{
			inventorySlot = (InventorySlot)GD.Load<PackedScene>(inventorySlotPath).Instantiate();
			inventorySlot.Name = $"Slot{i}";
			flowContainer.AddChild(inventorySlot);

			inventorySlots[i] = inventorySlot;
		}
	}

	public int PutToInventory(string itemType, int amount)
	{
		for (int i = 0; i < inventorySlots.Length; i++)
		{
			if (amount == 0) { break; }
			Label amountLabel = inventorySlots[i].GetNode<Label>("ResourceAmount");
			
			if (inventorySlots[i].itemType == "")
			{
				inventorySlots[i].itemType = itemType;
				amountLabel.Text = amount.ToString();
				inventorySlots[i].UpdateSlotTexture(itemType);
				amount = 0;
			}
			
			if (inventorySlots[i].itemType == itemType)
			{
				int slotAmount = int.Parse(amountLabel.Text);
				if (amount + slotAmount <= (int)items[itemType].maxStackSize)
				{
					amountLabel.Text = (amount + slotAmount).ToString();
					amount = 0;
				}
				else if (slotAmount != (int)items[itemType].maxStackSize)
				{
					amountLabel.Text = items[itemType].maxStackSize.ToString();
					amount = amount + slotAmount - (int)items[itemType].maxStackSize;
				}
			}
		}
		return amount;
	}
	

	public bool IsInInventory(string itemType, int amount)
	{
		if (itemType == "" || amount == 0) { return true; }

		for (int i = 0; i < inventorySlots.Length; i++)
		{
			if (itemType == inventorySlots[i].itemType)
			{
				int slotAmount = int.Parse(inventorySlots[i].GetNode<Label>("ResourceAmount").Text);
				if (amount <= slotAmount)
				{
					amount = 0;
				}
				else if (amount > slotAmount)
				{
					amount -= slotAmount;
				}

				if (amount == 0) { return true; }
			}
		}
		return false;
	}

	public void RemoveFromInventory(string itemType, int amount)
	{
		if (itemType == "" || amount == 0) { return; }

		for (int i = inventorySlots.Length - 1; i >= 0; i--)
		{
			Label amountLabel = inventorySlots[i].GetNode<Label>("ResourceAmount");
			
			if (itemType == inventorySlots[i].itemType)
			{
				if (amount >= int.Parse(amountLabel.Text))
				{
					amount -= int.Parse(amountLabel.Text);
					amountLabel.Text = "";
					inventorySlots[i].UpdateSlotTexture("");
					inventorySlots[i].itemType = "";
				}
				else 
				{
					amountLabel.Text = (int.Parse(amountLabel.Text) - amount).ToString();
					amount = 0;
				}
			}

			if (amount == 0) { return; }
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
