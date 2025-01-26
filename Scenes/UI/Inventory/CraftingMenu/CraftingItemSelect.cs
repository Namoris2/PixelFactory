using Godot;
using System;
using System.Collections.Generic;

public partial class CraftingItemSelect : Button
{
	[Export] public string itemType;
	bool mouseHover;
	Node parent;
	CraftingMenu craftingMenu;
	public Label itemName;
	public TextureRect itemIcon;
	dynamic item;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += SelectRecipe;
		ButtonDown += ButtonHeldDown;
		MouseEntered += MouseEnteredHandler;
		MouseExited += MouseExitedHandler;

		item = LoadFile.LoadJson("items.json")[itemType];

		itemName = GetNode<Label>("HBoxContainer/ItemName");
		itemName.Text = item.name.ToString();

		itemIcon = GetNode<TextureRect>("HBoxContainer/ItemIcon");
		itemIcon.Texture = main.GetTexture("items.json", itemType);

		craftingMenu = (CraftingMenu)GetTree().GetNodesInGroup("CraftingMenu")[0];
		parent = GetParent();

		if (Disabled)
		{
			craftingMenu.CallDeferred("ChangeRecipe", itemType);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SelectRecipe()
	{
		craftingMenu.ChangeRecipe(itemType);
	}

	private void ButtonHeldDown()
	{
		// #ffffff30
		itemName.Modulate = new Color("#ffffff30");
		itemIcon.Modulate = new Color("#ffffff30");
	}

	private void MouseEnteredHandler()
	{
		mouseHover = true;
	}

	private void MouseExitedHandler()
	{
		mouseHover = false;
	}
}
