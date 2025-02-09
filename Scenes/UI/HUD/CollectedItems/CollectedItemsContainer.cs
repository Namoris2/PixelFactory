using Godot;
using System;

public partial class CollectedItemsContainer : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		World tileMap = GetNode<World>("/root/main/World/TileMap");
		tileMap.CollectedItem += ShowCollectedItem;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ShowCollectedItem(string itemType = "", int amount = 0)
	{
		if (itemType == "" || amount == 0) { return; }
		CollectedItem collectedItem = GetNodeOrNull<CollectedItem>(itemType);

		if (collectedItem == null)
		{
			collectedItem = (CollectedItem)GD.Load<PackedScene>("res://Scenes/UI/HUD/CollectedItems/CollectedItem.tscn").Instantiate();
			collectedItem.Name = itemType;
			AddChild(collectedItem);
		}

		collectedItem.UpdateCollectedItemIndicator(itemType, amount);
	}
}
