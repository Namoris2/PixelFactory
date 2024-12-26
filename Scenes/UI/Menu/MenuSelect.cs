using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
		// loads 'buildings.json' file and parses in to dynamic object
		data = load.LoadJson($"{Type}s.json")[DisplayName];

		GetNode<Label>("Name").Text = data.name;
		
		AtlasTexture texture = new ();
		Vector2I size = new (1, 1);

		switch (Type)
		{
			case "building":
				this.Pressed += Build;
				size = new ((int)data.width, (int)data.height);

				MouseEntered += ShowBuildingInfo;
				MouseExited += HideBuildingInfo;
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

		texture.Atlas = GD.Load<Texture2D>($"res://Gimp/{char.ToUpper(Type[0]) + Type.Substring(1)}s/{Type}s.png");
		texture.Region = new Rect2I(atlasCoords[0] * 16, atlasCoords[1] * 16, size.X * 16, size.Y * 16);
		GetNode<TextureRect>("Texture").Texture = texture;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SelectRecipe()
	{
		World tileMap = GetNode<World>("/root/main/World/TileMap");
		BuildingInventory buildingInventory = GetNode<BuildingInventory>("/root/main/UI/Inventories/InventoryGrid/BuildingInventory");
		
		Vector2I coords = new Vector2I((int)buildingInventory.buildingInfo.coords[0], (int)buildingInventory.buildingInfo.coords[1]);
		tileMap.ChangeRecipe(DisplayName, coords);
		buildingInventory.ToggleInventory(true, buildingInventory.buildingInfo);
	}

	private void Build()
	{
		BuildMenu buildMenu = GetNode<BuildMenu>("/root/main/UI/BuildMenu");
		buildMenu.ToggleBuildMode();

		World tileMap = GetNode<World>("/root/main/World/TileMap");
		tileMap.selectedBuilding = DisplayName;
		tileMap.ToggleBuildMode(true);
		if (!(bool)data.canRotate) { tileMap.buildingRotation = 0; }
		
		else { GameEvents.rotatePopup.Show(); }
		GameEvents.camera.toggleZoom = true;
		GameEvents.toggleBuildMenuPopup.SetDefaultActionText();
		GameEvents.toggleInventoryPopup.Show();
	}

	private void ShowBuildingInfo()
	{
		BuildingInfo buildingInfo = (BuildingInfo)GetTree().GetNodesInGroup("BuildingInfo")[0];
		buildingInfo.ShowBuildingInfo(DisplayName);
	}
	
	private void HideBuildingInfo()
	{
		BuildingInfo buildingInfo = (BuildingInfo)GetTree().GetNodesInGroup("BuildingInfo")[0];
		buildingInfo.HideBuildingInfo();
	}
}
