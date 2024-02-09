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
					amount = (amount + slotAmount) - (int)items[itemType].maxStackSize;
				}
			}
		}
		return amount;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
