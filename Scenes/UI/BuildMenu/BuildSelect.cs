using Godot;
using System;

public partial class BuildSelect : Button
{
[Export]
public string BuildingName = "";
dynamic buildings;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadFile load = new();

		// loads 'buildigns.json' file and parses in to dynamic object
		buildings = load.LoadJson("buildings.json");
		
		GetNode<Label>("BuildingName").Text = buildings[BuildingName].name;
		this.Pressed += Build;

		Vector2I atlasCoords = new Vector2I((int)buildings[BuildingName].atlasCoords[0], (int)buildings[BuildingName].atlasCoords[1]);

		AtlasTexture texture = new ();
		texture.Atlas = GD.Load<Texture2D>("res://Gimp/buildings/buildings.png");
		texture.Region = new Rect2I(atlasCoords[0] * 16, atlasCoords[1] * 16, (int)buildings[BuildingName].width * 16, (int)buildings[BuildingName].height * 16);
		GetNode<TextureRect>("BuildingTexture").Texture = texture;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void Build()
	{
		BuildMenu buildMenu = GetNode<BuildMenu>("/root/main/UI/BuildMenu");
		buildMenu.ToggleBuildMode();

		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		tileMap.selectedBuilding = BuildingName;
		tileMap.BUILDINGMODE = false;
		tileMap.ToggleBuildMode();
	}
}
