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
	
	dynamic data;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// loads 'buildings.json' file and parses in to dynamic object
		data = LoadFile.LoadJson($"{Type}s.json")[DisplayName];

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
				data = LoadFile.LoadJson("items.json")[DisplayName];
				Type = "item";
				break;

			default:
				GD.PrintErr($"WRONG TYPE SELECTED! {DisplayName}, {Type}");
				break;
		}

		GetNode<TextureRect>("Texture").Texture = main.GetTexture(Type + "s.json", DisplayName);
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
		BuildMenu buildMenu = GetNode<BuildMenu>("/root/main/UI/Menus/BuildMenu");
		buildMenu.ToggleBuildMode();

		World tileMap = GetNode<World>("/root/main/World/TileMap");
		tileMap.selectedBuilding = DisplayName;
		tileMap.ToggleBuildMode(true);
		if (!(bool)data.canRotate) { tileMap.buildingRotation = 0; }
		
		else { GameEvents.rotatePopup.Show(); }
		GameEvents.camera.toggleZoom = true;
		GameEvents.toggleBuildMenuPopup.SetDefaultActionText();
		GameEvents.toggleInventoryPopup.Show();
		GameEvents.toggleDismantleModePopup.Show();
		GameEvents.toggleDismantleModePopup.SetCustomActionText();
		GameEvents.toggleResearchMenuPopup.Show();
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
