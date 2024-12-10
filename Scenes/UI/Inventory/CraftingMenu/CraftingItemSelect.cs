using Godot;
using System;
using System.Collections.Generic;

public partial class CraftingItemSelect : Button
{
	[Export]
	private string itemType;
	bool mouseHover;
	Node parent;
	CraftingMenu craftingMenu;
	public Label itemName;
	LoadFile load = new();
	dynamic item;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += SelectRecipe;
		MouseEntered += MouseEnteredHandler;
		MouseExited += MouseExitedHandler;

		item = load.LoadJson("items.json")[itemType];

		itemName = GetNode<Label>("ItemName");
		itemName.Text = item.name.ToString();

		AtlasTexture atlasTexture = new();
		Vector2I location = new Vector2I((int)item.atlasCoords[0], (int)item.atlasCoords[1]) * 16;

		atlasTexture.Atlas = GD.Load<Texture2D>($"res://Gimp/Items/items.png");
		atlasTexture.Region = new Rect2I(location[0], location[1], 16 , 16);

		GetNode<TextureRect>("ItemIcon").Texture = atlasTexture;

		craftingMenu = (CraftingMenu)GetTree().GetNodesInGroup("CraftingMenu")[0];
		parent = GetParent();

		if (Disabled)
		{
			craftingMenu.selectedRecipe = itemType;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Disabled)
		{
			// gray color
			itemName.AddThemeColorOverride("font_color", new Color("#888484"));
		}
		else if (mouseHover)
		{
			// yellow-ish color
			itemName.AddThemeColorOverride("font_color", new Color("#bda825"));
		}
		else
		{
			itemName.RemoveThemeColorOverride("font_color");
		}
	}

	private void SelectRecipe()
	{
		for (int i = 0; i < parent.GetChildCount(); i++)
		{
			CraftingItemSelect itemRecipe = (CraftingItemSelect)parent.GetChild(i);
			itemRecipe.Disabled = false;

		}

		craftingMenu.ChangeRecipe(itemType);
		Disabled = true;
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
