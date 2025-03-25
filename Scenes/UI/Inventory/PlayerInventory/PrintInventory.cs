using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PrintInventory : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += PrintInventorySlots;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void PrintInventorySlots()
	{
		GD.Print();
		PlayerInventory playerInventory = (PlayerInventory)GetParent();
		List<InventorySlot> slots = playerInventory.inventorySlots;

		for (int i = 0; i < slots.Count; i++)
		{
			InventorySlot inventorySlot = slots[i];
			string slotName = inventorySlot.Name;
			string itemName = inventorySlot.itemType;
			string itemAmount = inventorySlot.GetNode<Label>("ResourceAmount").Text;
			
			if (itemName == "") { itemName = "none"; }
			if (itemAmount == "") { itemAmount = "0"; }

			GD.Print($"{slotName}\nItemType: {itemName}, Amount: {itemAmount}");
		}
	}
}
