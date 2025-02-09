using Godot;
using System;

public partial class CollectedItem : HBoxContainer
{
	[Export] string itemType;
	[Export] int amount = 0;

	Label itemName;
	Label itemAmount;
	Label totalAmount;
	TextureRect icon;
	Timer timer;
	PlayerInventory playerInventory;

	dynamic items;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		itemName = GetNode<Label>("ItemName");
		itemAmount = GetNode<Label>("ItemAmount");
		totalAmount = GetNode<Label>("TotalAmount");
		icon = GetNode<TextureRect>("Icon");
		timer = GetNode<Timer>("Timer");
		timer.Timeout += RemoveCollectedItemIndicator;

		playerInventory = GetNode<PlayerInventory>("/root/main/UI/Inventories/InventoryGrid/PlayerInventory");

		items = LoadFile.LoadJson("items.json");

		//UpdateCollectedItemInfo(itemType, amount, false);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void UpdateCollectedItemIndicator(string itemType, int collectedAmount, bool addToExistingAmount = true)
	{
		if (addToExistingAmount) { amount += collectedAmount; }
		itemName.Text = items[itemType].name.ToString();
		itemAmount.Text = "+" + amount;
		totalAmount.Text = $"({playerInventory.GetItemAmount(itemType).ToString()})";
		icon.Texture = main.GetTexture("items.json", itemType);
		timer.Start();
	}

	private void RemoveCollectedItemIndicator()
	{
		QueueFree();
	}
}
