using Godot;
using System;

public partial class CollectedItem : HBoxContainer
{
	[Export] string itemType;
	[Export] int amount = 0;

	Label itemName;
	Label itemAmount;
	TextureRect icon;
	dynamic items;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		itemName = GetNode<Label>("ItemName");
		itemAmount = GetNode<Label>("ItemAmount");
		icon = GetNode<TextureRect>("Icon");

		items = LoadFile.LoadJson("items.json");

		ShowCollectedItem(itemType, amount, false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ShowCollectedItem(string itemType, int collectedAmount, bool addToExistingAmount = true)
	{
		if (addToExistingAmount) { amount += collectedAmount; }
		itemName.Text = items[itemType].name.ToString();
		itemAmount.Text = "+" + amount;
	}
}
