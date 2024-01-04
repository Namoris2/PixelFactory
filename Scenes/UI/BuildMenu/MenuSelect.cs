using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;

public partial class MenuSelect : Button
{
	[Export]
	public string DisplayName = "";
	
	[Export (PropertyHint.Enum, "building,recipe")]
	public string Type = "";

	LoadFile load = new();
	dynamic data;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// loads 'buildigns.json' file and parses in to dynamic object
		data = load.LoadJson($"{Type}s.json")[DisplayName];

		GetNode<Label>("Name").Text = data.name;
		
		AtlasTexture texture = new ();
		Vector2I size = new (1, 1);

		switch (Type)
		{
			case "building":
				this.Pressed += Build;
				size = new ((int)data.width, (int)data.height);
				break;

			case "recipe":
				this.Pressed += SelectRecipe;
				data = load.LoadJson("items.json")[DisplayName];
				Type = "item";
				break;

			default:
				GD.PrintErr($"WRONG TYPE SELECTED! {DisplayName}, {Type}");
				break;
		}
		
		Vector2I atlasCoords = new Vector2I((int)data.atlasCoords[0], (int)data.atlasCoords[1]);

		texture.Atlas = GD.Load<Texture2D>($"res://Gimp/{Type}s/{Type}s.png");
		texture.Region = new Rect2I(atlasCoords[0] * 16, atlasCoords[1] * 16, size.X * 16, size.Y * 16);
		GetNode<TextureRect>("Texture").Texture = texture;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SelectRecipe()
	{
		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		BuildingInventory buildingInventory = GetNode<BuildingInventory>("/root/main/UI/Inventories/InventoryGrid/BuildingInventory");
		TabContainer tabContainer = GetNode<TabContainer>(buildingInventory.GetPath() + "/TabContainer");
		dynamic recipe = load.LoadJson("recipes.json")[DisplayName];

		buildingInventory.resourceProduction.Text = $"Recipe: {recipe.name.ToString()}";
		Vector2I coords = new Vector2I((int)buildingInventory.buildingInfo.coords[0], (int)buildingInventory.buildingInfo.coords[1]);
		tileMap.ChangeRecipe(DisplayName, coords);
		tabContainer.CurrentTab = 1;
		
		Array<Node> inputSlots = GetTree().GetNodesInGroup("InputSlots");
		Array<Node> outputSlots = GetTree().GetNodesInGroup("OutputSlots");
		Array<Node> singleOutputSlots = GetTree().GetNodesInGroup("SingleSlots");

		for (int i = 0; i < inputSlots.Count; i++)
		{
			InventorySlot slot = (InventorySlot)inputSlots[i];
			slot.Hide();
		}

		for (int i = 0; i < outputSlots.Count; i++)
		{
			InventorySlot slot = (InventorySlot)outputSlots[i];
			slot.Hide();
		}

		for (int i = 0; i < singleOutputSlots.Count; i++)
		{
			InventorySlot slot = (InventorySlot)singleOutputSlots[i];
			slot.Hide();
		}


		for (int i = 0; i < recipe.input.Count; i++)
		{
			InventorySlot slot = (InventorySlot)inputSlots[i];
			slot.Show();
		}

		for (int i = 0; i < recipe.output.Count; i++)
		{
			InventorySlot slot = (InventorySlot)outputSlots[i];
			slot.Show();
		}
	}

	private void Build()
	{
		BuildMenu buildMenu = GetNode<BuildMenu>("/root/main/UI/BuildMenu");
		buildMenu.ToggleBuildMode();

		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		tileMap.selectedBuilding = DisplayName;
		tileMap.BUILDINGMODE = false;
		tileMap.ToggleBuildMode();
	}
}
