using Godot;
using System;

public partial class PlayerInventory : Control
{
	FlowContainer flowContainer;
	private InventorySlot inventorySlot;
	
	[Export]
	private string inventorySlotPath = "res://Scenes/UI/Inventory/InventorySlot.tscn";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		flowContainer = GetNode<FlowContainer>("FlowContainer");
		inventorySlot = GetNode<InventorySlot>("FlowContainer/Slot0");
		CreateInventorySlots();
	}

	private void CreateInventorySlots()
	{
		for (int i = 0; i < 34; i++)
		{
			Node newSlot = inventorySlot.Duplicate();
			newSlot.Name = $"Slot{i + 1}";
			flowContainer.AddChild(newSlot);
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
