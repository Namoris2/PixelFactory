using Godot;
using System;
using System.Reflection.Metadata;

public partial class PlayerInventory : Control
{
	FlowContainer flowContainer;
	private InventorySlot inventorySlot;
	private string inventorySlotPath = "res://Scenes/UI/Inventory/InventorySlot.tscn";
	public InventorySlot[] inventorySlots;
	
	[Export]
	private int inventorySize = 0; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		inventorySlots = new InventorySlot[inventorySize];

		flowContainer = GetNode<FlowContainer>("FlowContainer");
		CreateInventorySlots();
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


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
