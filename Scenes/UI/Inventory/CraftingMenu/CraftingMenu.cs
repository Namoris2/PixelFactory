using Godot;
using Godot.Collections;
using System;

public partial class CraftingMenu : Control
{
	PlayerInventory playerInventory;
	public string selectedRecipe = "";
	LoadFile load = new();
	dynamic items;
	dynamic recipes;
	dynamic recipe;

	Array<Node> inputItems;
	Array<Node> outputItems;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playerInventory = GetNode<PlayerInventory>("../PlayerInventory");
		items = load.LoadJson("items.json");
		recipes = load.LoadJson("recipes.json");

		inputItems = GetTree().GetNodesInGroup("CraftingInputItems");
		outputItems = GetTree().GetNodesInGroup("CraftingOutputItems");

		GetNode<Button>("CraftingBackground/Craft").Pressed += Craft;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ChangeRecipe(string itemType)
	{
		selectedRecipe = itemType;
		recipe = recipes[itemType];

		for (int i = 1; i < inputItems.Count; i++)
		{
			InventorySlot slot = (InventorySlot)inputItems[i];
			slot.Hide();
		}

		for (int i = 1; i < outputItems.Count; i++)
		{
			InventorySlot slot = (InventorySlot)outputItems[i];
			slot.Hide();
		}



		for (int i = 0; i < recipe.input.Count; i++)
		{
			InventorySlot slot = (InventorySlot)inputItems[i];
			int amount = playerInventory.GetItemAmount(recipe.input[i].name.ToString());
			slot.resourceAmount.Text = $"{amount}/{recipe.input[i].amount}";
			slot.itemType = recipe.input[i].name.ToString();
			slot.UpdateSlotTexture(recipe.input[i].name.ToString());
			slot.Show();
		}

		for (int i = 0; i < recipe.output.Count; i++)
		{
			InventorySlot slot = (InventorySlot)outputItems[i];
			slot.resourceAmount.Text = recipe.output[i].amount.ToString();
			slot.itemType = recipe.output[i].name.ToString();
			slot.UpdateSlotTexture(recipe.output[i].name.ToString());
			slot.Show();
		}
	}

	private void Craft()
	{
		bool hasItems = true;
		bool hasSpace = true;

		for (int i = 0; i < recipe.input.Count; i++)
		{
			hasItems &= playerInventory.GetItemAmount(recipe.input[i].name.ToString()) >= (int)recipe.input[i].amount;
		}

		for (int i = 0; i < recipe.output.Count; i++)
		{
			hasSpace &= playerInventory.HasSpace(recipe.output[i].name.ToString(), (int)recipe.output[i].amount);
		}

		if (hasItems && hasSpace)
		{
			for (int i = 0; i < recipe.input.Count; i++)
			{
				playerInventory.RemoveFromInventory(recipe.input[i].name.ToString(), (int)recipe.input[i].amount);
				
				InventorySlot slot = (InventorySlot)inputItems[i];
				int amount = playerInventory.GetItemAmount(recipe.input[i].name.ToString());
				slot.resourceAmount.Text = $"{amount}/{recipe.input[i].amount}";
			}

			for (int i = 0; i < recipe.output.Count; i++)
			{
				playerInventory.PutToInventory(recipe.output[i].name.ToString(), (int)recipe.output[i].amount);
			}
		}
	}
}
