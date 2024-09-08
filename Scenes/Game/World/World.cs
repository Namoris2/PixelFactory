using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using System.Linq;
using System.Dynamic;
using System.Data;
using System.Security.AccessControl;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Threading;
using System.Globalization;
using System.Diagnostics.Tracing;

public partial class World : Godot.TileMap
{
	[Signal]
	public delegate void ResourcesUpdatedEventHandler();

	[Signal]
	public delegate void UpdateResourceInfoEventHandler(string resourceName);

	[Signal]
	public delegate void ToggleInventoryEventHandler(bool InvenotryToggle, string info, Leftovers leftovers);

	[Signal]
	public delegate void UpdateBuildingProgressEventHandler(string info);

	[Signal]
	public delegate void CreateParticleEventHandler(Vector2I coords, string type, string resource);

	[Signal]
	public delegate void RemoveParticleEventHandler(Vector2I coords);

	[Signal]
	public delegate void CreateBuildingPartEventHandler(Vector2I coords, string type, int rotation, bool createdBuilding);

	[Signal]
	public delegate void RemoveBuildingPartEventHandler(Vector2I coords, string type);
	
	[Signal]
	public delegate void CreateAnimatedBuildingPartEventHandler(Vector2I coords, string type, int rotation);

	[Signal]
	public delegate void RemoveAnimatedBuildingPartEventHandler(Vector2I coords, string type);

	[Export]
	private bool worldGeneration = true;

	[Export]
	int mapRadius = 250;
	int seed;

	public bool UITOGGLE = false;
	public bool BUILDINGMODE = false;
	public bool DISMANTLEMODE = false;

	Vector2I previousCellPosition = new (0, 0);

	string groundResourceName;
	public TileData buildingsData;
	public Vector2I cellPositionByMouse;

	// setting default grid cursor parameters
	int tileMapId = 100;
	int tileMapLayer = 3;
	Vector2I atlasPosition = new (0, 0);
	string[] buildingsCoords = new string[] {};
	public List<dynamic> buildingsInfo = new();
	public string selectedBuilding;
	public int buildingRotation = 0;
	dynamic resourcesHarvestedByHand;

	int resourceAmount = 100000;
	dynamic buildings;
	dynamic items;
	dynamic groundResources;
	dynamic recipes;

	Vector2I lastPlayerPosition = new (0, 0);
	PlayerInventory playerInventory;
	BuildingInventory buildingInventory;
	GenerateWorld generateWorld;

	List<Thread> runningThreads = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// hide cursor
		//Input.MouseMode = Input.MouseModeEnum.Hidden;
		
		EmitSignal(SignalName.ResourcesUpdated, resourceAmount);

		LoadFile load = new();

		// loads 'buildings.json' file and parses in to dynamic object
		buildings = load.LoadJson("buildings.json");
		resourcesHarvestedByHand = buildings.handHarvest;

		items = load.LoadJson("items.json");
		groundResources = load.LoadJson("groundResources.json");

		recipes = load.LoadJson("recipes.json");

		playerInventory = GetNode<PlayerInventory>("/root/main/UI/Inventories/InventoryGrid/PlayerInventory");
		buildingInventory = playerInventory.GetNode<BuildingInventory>("../BuildingInventory");

		generateWorld = GetNode<GenerateWorld>("/root/GenerateWorld");

		seed = GetNode<main>("/root/GameInfo").seed;
		Load();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Player player = GetNode<Player>("../Player");
		Vector2I playerPosition = new((int)Math.Floor(player.Position.X / 64), (int)Math.Floor(player.Position.Y / 64));
		playerPosition = new(playerPosition.X - playerPosition.X % generateWorld.chunkSize, playerPosition.Y - playerPosition.Y % generateWorld.chunkSize);


		if (playerPosition != lastPlayerPosition)
		{
			GenerateChunks(playerPosition);
			/*Thread generateWorld = new(() => GenerateChunks(playerPosition));
			runningThreads.Add(generateWorld);
			generateWorld.Start();*/

			lastPlayerPosition = playerPosition;
		}


		// if any inventory is opened any of the actions bellow won't work
		if (UITOGGLE) { return; }
		
		PauseMenu pauseMenu = GetNode<PauseMenu>("/root/main/UI/PauseMenu");
		pauseMenu.CanPause = !UITOGGLE && !BUILDINGMODE;

		// getting mouse position with TileMap coordinates
		cellPositionByMouse = GetMousePosition();

		// getting tileMap data by mouse coordinates that hovers over TileMap cell
		TileData groundData = GetCellTileData(0, cellPositionByMouse);
		buildingsData = GetCellTileData(1, cellPositionByMouse);

		if (groundData == null) { return; }

		groundResourceName = (string)groundData.GetCustomData("resourceName");

		string worldInfo;
		
		/*if (buildingsData != null && GetBuildingInfo(cellPositionByMouse).buildingType.ToString() == "machine")
		{
			dynamic buildingDisplayInfo = GetBuildingInfo(cellPositionByMouse);

			worldInfo = $"Building: {buildingDisplayInfo.name} \nProgress: {(int)(buildingDisplayInfo.productionProgress * 100)}%";	
		}
		else if (buildingsData != null && (GetBuildingInfo(cellPositionByMouse).buildingType.ToString() == "belt" || GetBuildingInfo(cellPositionByMouse).buildingType.ToString() == "beltArm"))
		{
			dynamic buildingDisplayInfo = GetBuildingInfo(cellPositionByMouse);

			worldInfo = $"Building: {buildingDisplayInfo.name} \nProgress: {(int)(buildingDisplayInfo.moveProgress * 100)}% \nItem: {buildingDisplayInfo.item}";	
		}*/

		if (buildingsData != null)
		{
			dynamic buildingData = buildings[GetBuildingInfo(cellPositionByMouse).type.ToString()];
			worldInfo = $"Building: {buildingData.name}";
		}
		else
		{
			worldInfo = $"Resource: {groundResources[groundResourceName].name}";
		}

		worldInfo = $"Coordinates: {cellPositionByMouse[0]}, {cellPositionByMouse[1]}\n{worldInfo}";
		
		EmitSignal(SignalName.UpdateResourceInfo, worldInfo);
		
		// moves square cursor to tiles
		CursorTexture();

		if (BUILDINGMODE)
		{
			if (Input.IsActionPressed("Use", true))
			{
				// builds a building
				Build();
			}
		}
		if (!BUILDINGMODE && !DISMANTLEMODE && !UITOGGLE)
		{
			if(Input.IsActionPressed("Use"))
			{
				// farming resources
				FarmResources(GetBuildingInfo(cellPositionByMouse));
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ToggleDismantleMode"))
		{
			ToggleDismantleMode();
		}

		// only if is 'BUILDINGMODE' toggled
		if (BUILDINGMODE)
		{
			// if selected building can rotate and 'Rotate' input is called, it rotates building
			if (@event.IsActionPressed("Rotate") && (bool)buildings[selectedBuilding].canRotate)
			{
				RotateBuilding();
			}
		}
			
		// only if is 'DISMANTLEMODE' toggled
		if (DISMANTLEMODE)
		{
			if (@event.IsActionPressed("Use"))
			{
				Dismantle();
			}
		}

		if (!BUILDINGMODE){}
		if (!DISMANTLEMODE){}

		// only if both 'BUILDINGMODE' and 'DISMANTLEMODE' aren't toggled
		if (!BUILDINGMODE && !DISMANTLEMODE)
		{
		}

	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2I nextCoords;
		Vector2I previousCoords;
		dynamic nextBuilding;
		dynamic previousBuilding;
		string itemName;
		Item item;

		for (int i = 0; i < buildingsInfo.Count; i++)
		{
			if (buildingsInfo[i].buildingType == "buildingPart") { continue; }

			dynamic buildingData = buildings[buildingsInfo[i].type.ToString()];
			switch(buildingsInfo[i].buildingType.ToString())
			{
				case "machine":
					if (buildingsInfo[i].recipe.ToString() == "none") { continue; } // if no recipe is selected, skips this building

					dynamic recipe = recipes[buildingsInfo[i].recipe.ToString()];
					float productionMultiplier = 1;

					if (BuildingSlotValidate(buildingsInfo[i], recipe))
					{
						if (buildingsInfo[i].type.ToString().Contains("Drill")) { productionMultiplier = (float)buildingData.productionMultiplier * (float)buildingsInfo[i].tiles; }
						buildingsInfo[i].productionProgress += productionMultiplier * (double)recipe.cyclesPerMinute / 60 * delta;

						if (buildingsInfo[i].productionProgress >= 1) // if 'productionProgress' is full items will be added and removed according to machine's recipe
																	  // and resets 'productionProgress' 
						{
							buildingsInfo[i].productionProgress = 0;

							for (int j = 0; j < recipe.input.Count; j++)
							{
								buildingsInfo[i].inputSlots[j].amount -= recipe.input[j].amount;
							}	

							for (int j = 0; j < recipe.output.Count; j++)
							{
								buildingsInfo[i].outputSlots[j].amount += recipe.output[j].amount;
							}
						}
					}

					break;
				
				case "belt":
					// gets the next belt
					nextCoords = new Vector2I((int)buildingsInfo[i].coords[0] + (int)buildingsInfo[i].nextPosition[0], (int)buildingsInfo[i].coords[1] + (int)buildingsInfo[i].nextPosition[1]);
					nextBuilding = GetBuildingInfo(nextCoords);
						
					// moves item on the belt
					if (buildingsInfo[i].item.ToString() != "" && (double)buildingsInfo[i].moveProgress < 1)
					{
						buildingsInfo[i].moveProgress += (double)buildingData.speed / 60 * delta;

						itemName = $"{buildingsInfo[i].coords[0]}x{buildingsInfo[i].coords[1]}";
						item = GetNode<Item>(itemName);

						if (item.parentBuilding != null) 
						{  
							item.parentBuilding = null;
						}
					}

					if ((double)buildingsInfo[i].moveProgress > 1) { buildingsInfo[i].moveProgress = 1; }
					
					// moves item to the next belt
					if (CanTBeltTransfer(buildingsInfo[i]))
					{
						itemName = $"{buildingsInfo[i].coords[0]}x{buildingsInfo[i].coords[1]}";
						item = GetNodeOrNull<Item>(itemName);
				
						item.destination = nextCoords * 64;
						item.Name = $"{nextCoords[0]}x{nextCoords[1]}";
						item.speed = 64 / (60f / (int)buildingData.speed);

						item.GetNode<Sprite2D>("ItemHolder").Hide();
						item.ZIndex = 0;

						nextBuilding.item = buildingsInfo[i].item;
						buildingsInfo[i].item = "";
						buildingsInfo[i].moveProgress = 0;
					}
					break;

				case "beltArm":
					previousCoords = new Vector2I((int)buildingsInfo[i].coords[0] + (int)buildingsInfo[i].previousPosition[0], (int)buildingsInfo[i].coords[1] + (int)buildingsInfo[i].previousPosition[1]);
					nextCoords = new Vector2I((int)buildingsInfo[i].coords[0] + (int)buildingsInfo[i].nextPosition[0], (int)buildingsInfo[i].coords[1] + (int)buildingsInfo[i].nextPosition[1]);

					previousBuilding = GetBuildingInfo(previousCoords);
					nextBuilding = GetBuildingInfo(nextCoords);

					if (buildingsInfo[i].item.ToString() != "" && (double)buildingsInfo[i].moveProgress < 1 && nextBuilding != null)
					{
						buildingsInfo[i].moveProgress += (double)buildingData.speed / 60 * delta;
					}

					if ((double)buildingsInfo[i].moveProgress > 1) { buildingsInfo[i].moveProgress = 1; }
					
					//taking item from building
					if ((double)buildingsInfo[i].moveProgress == 0 && CanArmTransfer(buildingsInfo[i]))
					{
						if (previousBuilding.buildingType.ToString() == "machine") // machine
						{
							buildingsInfo[i].item = previousBuilding.outputSlots[0].resource; 
							previousBuilding.outputSlots[0].amount -= 1;
							CreateItem(previousCoords, nextCoords, buildingsInfo[i].item.ToString(), (int)buildingData.speed * 2, parentBuilding: new Vector2I((int)buildingsInfo[i].coords[0], (int)buildingsInfo[i].coords[1]));
						}
						else if (previousBuilding.buildingType.ToString() == "storage") // storage
						{
							for (int j = previousBuilding.slots.Count - 1; j >= 0; j--)
							{
								//GD.Print(previousBuilding.slots[j].resource, previousBuilding.slots[j].amount); 
								if (previousBuilding.slots[j].resource.ToString() != "")
								{
									buildingsInfo[i].item = previousBuilding.slots[j].resource.ToString();
									previousBuilding.slots[j].amount -= 1;
									if ((int)previousBuilding.slots[j].amount == 0) { previousBuilding.slots[j].resource = ""; }
									CreateItem(previousCoords, nextCoords, buildingsInfo[i].item.ToString(), (float)buildingData.speed * 2, parentBuilding: new Vector2I((int)buildingsInfo[i].coords[0], (int)buildingsInfo[i].coords[1]));
									break;
								}
							}
							//GD.Print();
						}
						else if (previousBuilding.buildingType.ToString() == "belt") // belt
						{
							buildingsInfo[i].item = previousBuilding.item;
							previousBuilding.item = "";
							previousBuilding.moveProgress = 0;

							itemName = $"{previousCoords[0]}x{previousCoords[1]}";
							item = GetNode<Item>(itemName);

							item.destination = nextCoords * 64;
							item.speed = 64 / (60 / (float)buildingData.speed) * 2;
							item.Name = $"{buildingsInfo[i].coords[0]}x{buildingsInfo[i].coords[1]}";
							item.parentBuilding = new ((int)buildingsInfo[i].coords[0], (int)buildingsInfo[i].coords[1]);
							item.GetNode<Sprite2D>("ItemHolder").Show();
							item.ZIndex = 1;
						}

						if (nextBuilding.buildingType.ToString() == "belt")
						{
							nextBuilding.item = buildingsInfo[i].item;
							item = GetNode<Item>($"{buildingsInfo[i].coords[0]}x{buildingsInfo[i].coords[1]}");
							item.Name = $"{nextCoords[0]}x{nextCoords[1]}";
						}
					}
					// putting item to building/belt
					else if ((double)buildingsInfo[i].moveProgress >= 1)
					{
						if (nextBuilding.buildingType.ToString() != "belt")
						{
							itemName = $"{buildingsInfo[i].coords[0]}x{buildingsInfo[i].coords[1]}";
							item = GetNode<Item>(itemName);
							Vector2I coords = new ((int)buildingsInfo[i].coords[0], (int)buildingsInfo[i].coords[1]);

							if (coords != item.parentBuilding) { break; }
							if (nextBuilding.buildingType.ToString() == "machine") { nextBuilding.inputSlots[0].amount += 1; }
							
							else
							{
								for (int j = 0; j < nextBuilding.slots.Count; j++)
								{
									if (nextBuilding.slots[j].resource.ToString() == "")
									{
										nextBuilding.slots[j].amount = 1;
										nextBuilding.slots[j].resource = buildingsInfo[i].item;
										break;
									}
									else if (buildingsInfo[i].item.ToString() == nextBuilding.slots[j].resource.ToString() && (int)nextBuilding.slots[j].amount < (int)items[nextBuilding.slots[j].resource.ToString()].maxStackSize)
									{
										nextBuilding.slots[j].amount += 1;
										break;
									}
								}
							}

							item.QueueFree();
						}

						buildingsInfo[i].item = "";
						buildingsInfo[i].moveProgress = 0;
					}
					
				break;					
			}

			// if inventory is opened, data will be sent to the inventory to show on screen
			if (buildingInventory.Visible && GetBuildingInfo(cellPositionByMouse) != null)
			{
				buildingInventory.UpdateInventory(buildingsInfo[i]);
			}
		}
	}

	public System.Collections.Generic.Dictionary<string, dynamic> Save()
    {
		List<dynamic> savedBuildings = new();
		List<dynamic> savedItems  = new();
		List<dynamic> savedLeftovers  = new();

		foreach (dynamic building in buildingsInfo)
		{
			if (building.buildingType != "buildingPart")
			{
				savedBuildings.Add(building);
			}
		}

		Array<Node> items = GetTree().GetNodesInGroup("Items");
		for (int i = 0; i < items.Count; i++)
		{
			Item item = (Item)items[i];
			System.Collections.Generic.Dictionary<string, dynamic> savedItem = new()
            {
                { "name", item.itemType },
				{ "position", new Array<float>() {item.Position.X / 64, item.Position.Y / 64} },
                { "destination", new Array<float>() {item.destination.X / 64 , item.destination.Y / 64} },
                { "speed", 60 * (item.speed / 64) },
				{ "parentBuilding", item.parentBuilding },
            };
			savedItems.Add(savedItem);
        }

		Array<Node> leftovers = GetTree().GetNodesInGroup("Leftovers");
		for (int i = 0; i < leftovers.Count; i++)
		{
			Leftovers box = (Leftovers)leftovers[i];
			System.Collections.Generic.Dictionary<string, dynamic> savedLeftoverBox = new()
            {
				{ "position", new Array<float>() {box.Position.X, box.Position.Y} },
				{ "items", box.items },
            };
			savedLeftovers.Add(savedLeftoverBox);
		}


        System.Collections.Generic.Dictionary<string, dynamic> savedData = new()
        {
            { "seed", seed },
            { "buildings", savedBuildings },
			{ "items", savedItems },
			{ "leftovers", savedLeftovers }
        };

		GD.Print("World saved");
		return savedData;
	}

	private void Load()
	{
        LoadingScreen loadingScreen = GetNodeOrNull<LoadingScreen>("/root/LoadingScreen");
		string savePath = GetNode<main>("/root/GameInfo").savePath;

		dynamic savedData = null;
		Vector2I playerPosition = new (0, 0);
        if (loadingScreen != null && loadingScreen.loadingSave && Godot.FileAccess.FileExists(savePath))
		{
			Godot.FileAccess saveFile = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Read);
			string savedGame = saveFile.GetAsText();
			saveFile.Close();

			savedData = JsonConvert.DeserializeObject(savedGame);
			dynamic worldData = savedData[Name];

			seed = (int)worldData.seed;

			dynamic loadedBuildings = worldData.buildings;
			foreach (dynamic building in loadedBuildings)
			{
				Vector2I position = new ((int)building.coords[0], (int)building.coords[1]);
				buildingRotation = (int)building.rotation;

				if (building.type.ToString() == "drill")
				{
					building.type = "smallDrill";
					building.Add("tiles", 1);
				}

				dynamic buildingData = buildings[building.type.ToString()];
				if (building.ContainsKey("atlasCoords")) 
				{ 
					dynamic trimmedBuilding = TrimBuildingData(building); 
					CreateBuilding(trimmedBuilding, buildingData, position);
					buildingsInfo.Add(trimmedBuilding);
					continue;
				}

				CreateBuilding(building, buildingData, position);
				buildingsInfo.Add(building);
			}
			buildingRotation = 0;

			dynamic savedItems = worldData.items;

			foreach (dynamic item in savedItems)
			{
				Vector2 coords = new((float)item.position[0], (float)item.position[1]);
				Vector2I destination = new((int)item.destination[0], (int)item.destination[1]);

				if (item.parentBuilding != null)
				{
					Vector2I? parentBuilding = new((int)item.parentBuilding.X, (int)item.parentBuilding.Y);
					CreateItem(coords, destination, item.name.ToString(), (int)item.speed, parentBuilding: parentBuilding);
					continue;
				}
				CreateItem(coords, destination, item.name.ToString(), (int)item.speed);
			}

			dynamic savedLeftovers = worldData.leftovers;
			foreach (var leftovers in savedLeftovers)
			{
				Leftovers instantiatedLeftovers = (Leftovers)GD.Load<PackedScene>("res://Scenes/Game/World/Leftovers/Leftovers.tscn").Instantiate();
				Vector2 position = new ((int)leftovers.position[0], (int)leftovers.position[1]);
				instantiatedLeftovers.GlobalPosition = position;
				instantiatedLeftovers.items = leftovers.items.ToObject<List<LeftoversSlot>>();
				AddChild(instantiatedLeftovers);
			}
		}

		if (savedData != null)
		{
			Vector2I savedPosition = new((int)savedData.Player.X, (int)savedData.Player.Y);
			playerPosition = savedPosition / 64;
			playerPosition = new(playerPosition.X - playerPosition.X % generateWorld.chunkSize, playerPosition.Y - playerPosition.Y % generateWorld.chunkSize);
			lastPlayerPosition = playerPosition;
		}

		GenerateChunks(playerPosition);
	}

	public void Load(dynamic data) {} // does nothing, just so 'Call' method does't have error

	private void GenerateChunks(Vector2I playerPosition)
	{
		generateWorld.GenerateResource(this, seed, "Grass", playerPosition, true);
		generateWorld.GenerateResource(this, seed, "IronOre", playerPosition);
		generateWorld.GenerateResource(this, seed, "CopperOre", playerPosition);
	}

	public Vector2I GetMousePosition()
	{
		var mousePosition = GetGlobalMousePosition();
		//Vector2I cellPositionByMouse = new ((int)Math.Floor(mousePosition[0] / (4 * 16)), (int)Math.Floor(mousePosition[1]  / (4 * 16)));
		Vector2I cellPositionByMouse = new ((int)mousePosition[0] / (4 * 16), (int)mousePosition[1]  / (4 * 16));

		if (mousePosition[0] < 0)
		{
			cellPositionByMouse = new Vector2I((int)(cellPositionByMouse[0] - 1), (int)(cellPositionByMouse[1]));
		}

		if (mousePosition[1] < 0)
		{
			cellPositionByMouse = new Vector2I((int)(cellPositionByMouse[0]), (int)(cellPositionByMouse[1] - 1));
		}

		return cellPositionByMouse;
	}

	private void CursorTexture()
	{
		ClearLayer(tileMapLayer);
		
		if (BUILDINGMODE)
		{
			Vector2I atlasCoords = new Vector2I((int)buildings[selectedBuilding].atlasCoords[0] + buildingRotation, (int)buildings[selectedBuilding].atlasCoords[1]);
			SetCell(tileMapLayer, cellPositionByMouse, tileMapId, atlasCoords);

			Node2D constructingPart = (Node2D)GetTree().GetFirstNodeInGroup("constructingPart");
			if (constructingPart != null)
			{
				constructingPart.Position = cellPositionByMouse * 64;
				//GD.Print(constructingPart.Position);
			}

			if ((bool)buildings[selectedBuilding].hasAdditionalAtlasPosition)
			{
				for (int i = 0; i < buildings[selectedBuilding].additionalAtlasPosition.Count; i++)
					{
						Vector2I additionalAtlasPosition = new Vector2I((int)buildings[selectedBuilding].additionalAtlasPosition[i][0], (int)buildings[selectedBuilding].additionalAtlasPosition[i][1]);
						SetCell(tileMapLayer, cellPositionByMouse + additionalAtlasPosition, tileMapId, atlasCoords + additionalAtlasPosition);
					}
			}
		}
		else {
			SetCell(tileMapLayer, cellPositionByMouse, tileMapId, atlasPosition);
		}

	}

	private void FarmResources(dynamic building)
	{
		if (GroundResourceValidate(resourcesHarvestedByHand.canBeUsedOn, groundResourceName) && Input.IsActionJustPressed("Use") && buildingsData == null)
		{
			playerInventory.PutToInventory(groundResourceName, 1);
		}

		if (buildingsData != null && building.buildingType.ToString() == "machine" && (Input.IsActionPressed("TakeAll") || Input.IsActionJustPressed("Use")) ) 
		{
			building.outputSlots[0].amount = playerInventory.PutToInventory(building.outputSlots[0].resource.ToString(), (int)building.outputSlots[0].amount);
		}
			
	}

	private void RotateBuilding()
	{
		buildingRotation = (buildingRotation + 1) % (int)buildings[selectedBuilding].rotationAmount;
		
		Node2D constructingPart = (Node2D)GetTree().GetFirstNodeInGroup("constructingPart");
		if (constructingPart != null)
		{
			constructingPart.GetNode<Node2D>("Part").RotationDegrees += 90;
		}
	}

	public void ToggleBuildMode(bool? toggle = null)
	{
		Node2D constructingPart = (Node2D)GetTree().GetFirstNodeInGroup("constructingPart");
		if (constructingPart != null)
		{
			constructingPart.QueueFree();
		}
		
		if (toggle == null)
		{
			BUILDINGMODE = !BUILDINGMODE;
		}
		else
		{
			BUILDINGMODE = (bool)toggle;
		}
		DISMANTLEMODE = false;
		
		ClearLayer(tileMapLayer);
		tileMapLayer = 2;
		tileMapId = 1;
		atlasPosition = new (0, 0);
		
		if (!BUILDINGMODE)
		{
			// draws square crosshair to TileMap
			tileMapLayer = 3;
			tileMapId = 100;
			atlasPosition = new (0, 0);
			buildingRotation = 0;
			
		}
		else
		{
			EmitSignal(SignalName.CreateBuildingPart, cellPositionByMouse, selectedBuilding, 0, false);
		}
	}

	public void ToggleDismantleMode(bool? toggle = null)
	{
		if (toggle == null)
		{
			DISMANTLEMODE = !DISMANTLEMODE;
		}
		else
		{
			DISMANTLEMODE = (bool)toggle;
		}
		BUILDINGMODE = false;

		ClearLayer(tileMapLayer);
		tileMapLayer = 3;
		tileMapId = 100;
		atlasPosition = new (0, 0);	

		if (DISMANTLEMODE)
		{
			// draws cross crosshair to TileMap
			atlasPosition = new (1, 0);

			Node2D constructingPart = (Node2D)GetTree().GetFirstNodeInGroup("constructingPart");
			if (constructingPart != null)
			{
				constructingPart.QueueFree();
			}
		}
	}

	private void Build()
	{
		System.Collections.Generic.Dictionary<string, dynamic> groundInfo = GetGroundInfo(buildings[selectedBuilding]);
		if (groundInfo["canBuild"] && HasItemsToBuild(buildings[selectedBuilding].cost))
		{
			string buildingsJson = JsonConvert.SerializeObject(buildings);
			dynamic building = JsonConvert.DeserializeObject<dynamic>(buildingsJson);
			building = building[selectedBuilding];
			building.coords[0] = cellPositionByMouse[0];
			building.coords[1] = cellPositionByMouse[1];
			
			if  (building.type.ToString().Contains("Drill"))
			{
				building.outputSlots[0].resource = groundResourceName;
				building.recipe = groundInfo["resource"];
				building.tiles = groundInfo["tiles"];
			}

			Node2D constructingPart = (Node2D)GetTree().GetFirstNodeInGroup("constructingPart");
			if (constructingPart != null)
			{
				Node2D part = constructingPart.GetNode<Node2D>("Part");
				buildingRotation = (int)(part.RotationDegrees % 360) / 90;
			}

			if ((bool)building.canRotate) { building.rotation = buildingRotation; }
			Vector2I nextPosition = new Vector2I(0, 0);
			
			switch (building.buildingType.ToString())
			{
				case "belt":
					switch (buildingRotation)
					{
						case 0:
							nextPosition = new Vector2I(1, 0);
							break;
						case 1:
							nextPosition = new Vector2I(0, 1);
							break;
						case 2:
							nextPosition = new Vector2I(-1, 0);
							break;
						case 3:
							nextPosition = new Vector2I(0, -1);
							break;
					}

					building.nextPosition[0] = nextPosition[0];
					building.nextPosition[1] = nextPosition[1];
					break;
				
				case "beltArm":
					int reach = (int)building.reach;
					switch (buildingRotation)
					{
						case 0:
							nextPosition = new Vector2I(reach, 0);
							break;
						case 1:
							nextPosition = new Vector2I(0, reach);
							break;
						case 2:
							nextPosition = new Vector2I(-reach, 0);
							break;
						case 3:
							nextPosition = new Vector2I(0, -reach);
							break;
					}

					building.nextPosition[0] = nextPosition[0];
					building.nextPosition[1] = nextPosition[1];

					building.previousPosition[0] = -1 * nextPosition[0];
					building.previousPosition[1] = -1 * nextPosition[1];
					break;
				
				case "storage":
				{
					for (int i = 0; i < (int)building.slotsAmount; i++)
					{
						building.slots.Add(JsonConvert.DeserializeObject<dynamic>("{resource:\"\",amount:0}"));
					}
					//GD.Print(building.slots.Count);
					break;
				}
			}

			// removes cost items from inventory
			for (int i = 0; i < building.cost.Count; i++)
			{
				playerInventory.RemoveFromInventory(building.cost[i].resource.ToString(), (int)building.cost[i].amount);
			}

			building = TrimBuildingData(building);
			CreateBuilding(building, buildings[building.type.ToString()], cellPositionByMouse);
			
			buildingsInfo.Add(building);
			buildings = JsonConvert.DeserializeObject<dynamic>(buildingsJson);
		}
	}

	private System.Collections.Generic.Dictionary<string, dynamic> GetGroundInfo(dynamic building)
	{
		System.Collections.Generic.Dictionary<string, dynamic> info = new ()
		{
			{ "canBuild", false },
			{ "tiles", 0 },
			{ "resource", "" }
		};
		List<string> resources = new ();
		TileData data = GetCellTileData(1, cellPositionByMouse);
		string groundResource = (string)GetCellTileData(0, cellPositionByMouse).GetCustomData("resourceName");

		bool hasSpace = data == null; 
		resources.Add(groundResource);

		for (int i = 0; i < building.additionalAtlasPosition.Count; i++)
		{
			Vector2I additionalCoords = new((int)building.additionalAtlasPosition[i][0], (int)building.additionalAtlasPosition[i][1]);
			data = GetCellTileData(1, cellPositionByMouse + additionalCoords);
			groundResource = (string)GetCellTileData(0, cellPositionByMouse + additionalCoords).GetCustomData("resourceName");

			hasSpace &= data == null; 
			resources.Add(groundResource);
		}

		if (building.type.ToString().Contains("Drill"))
		{
			if (resources.Contains("Water"))
			{
				info["canBuild"] = false;
				return info;
			}

			resources.RemoveAll(resource => resource == "Grass");
			info["canBuild"] = hasSpace && resources.Count > 0;
			
			if (!info["canBuild"]) { return info; }

			if (hasSpace && !(bool)building.hasAdditionalAtlasPosition)
			{
				info["tiles"] = 1;
				info["resource"] = groundResource;
			}
			else
			{
				dynamic mostFrequentResource = resources
					.GroupBy(x => x)                   						 // Group elements by their value
					.Select(g => new { Element = g.Key, Count = g.Count() }) // Create a new object with the element and its count
					.OrderByDescending(g => g.Count)   						 // Order by descending count
					.First();  

				info["tiles"] = mostFrequentResource.Count;
				info["resource"] = mostFrequentResource.Element;
			}
		}
		else
		{
			info["canBuild"] = hasSpace && resources.Count == building.additionalAtlasPosition.Count + 1;
		}
		
		return info;
	}
	
	private dynamic TrimBuildingData(dynamic building)
	{
		JObject buildingData = new()
        {
            { "buildingType", building.buildingType },
            { "type", building.type },
			{ "coords", building.coords },
			{ "rotation", building.rotation },
        };

		switch (building.buildingType.ToString())
		{
			case "machine":
				buildingData.Add("recipe", building.recipe);
				buildingData.Add("productionProgress", building.productionProgress);
				buildingData.Add("inputSlots", building.inputSlots);
				buildingData.Add("outputSlots", building.outputSlots);

				if (building.type.ToString().Contains("Drill")) { buildingData.Add("tiles", building.tiles);}
				break;

			case "belt": case "beltArm":
				if (building.buildingType.ToString() == "beltArm") { buildingData.Add("previousPosition", building.previousPosition); }

				buildingData.Add("nextPosition", building.nextPosition);
				buildingData.Add("moveProgress", building.moveProgress);
				buildingData.Add("item", building.item);
				break;

			case "storage":
				buildingData.Add("slots", building.slots);
				break;
		}

		return buildingData;
	}

	private void CreateBuilding(dynamic building, dynamic buildingData, Vector2I cellPosition)
	{
		// Gets building's recipe
		string recipe = "";
		if (building.ContainsKey("recipe")) { recipe = building.recipe.ToString(); }
		
		// Adds particles and additional parts to building
		EmitSignal(SignalName.CreateParticle, cellPosition, building.type.ToString(), recipe);
		EmitSignal(SignalName.CreateAnimatedBuildingPart, cellPosition, building.type.ToString(), buildingRotation);
		EmitSignal(SignalName.CreateBuildingPart, cellPosition, building.type.ToString(), buildingRotation, true);

		string buildingsJson = JsonConvert.SerializeObject(buildings);
		
		if ((int)buildingData.rotationAmount == 1) { buildingRotation = 0; }
		SetCell(1, cellPosition, 1, new((int)buildingData.atlasCoords[0] + buildingRotation, (int)buildingData.atlasCoords[1]));	
		
		if((bool)buildingData.hasAdditionalAtlasPosition)
		{
			for (int i = 0; i < buildingData.additionalAtlasPosition.Count; i++)
			{
				Vector2I atlasCoords = new ((int)buildingData.atlasCoords[0], (int)buildingData.atlasCoords[1]);
				Vector2I additionalAtlasPosition = new ((int)buildingData.additionalAtlasPosition[i][0], (int)buildingData.additionalAtlasPosition[i][1]);
				
				dynamic buildingPart = JsonConvert.DeserializeObject<dynamic>(buildingsJson);;
				buildingPart = buildingPart.buildingPart;

				buildingPart.coords[0] = cellPosition[0] + additionalAtlasPosition[0];
				buildingPart.coords[1] = cellPosition[1] + additionalAtlasPosition[1];
				buildingPart.parentBuilding[0] = building.coords[0];
				buildingPart.parentBuilding[1] = building.coords[1];

				SetCell(1, cellPosition + additionalAtlasPosition, 1, atlasCoords + additionalAtlasPosition);
				buildingsInfo.Add(buildingPart);
			}
		}


	}

	private bool HasItemsToBuild (dynamic items)
	{
		bool hasItem = true;
		for (int i = 0; i < items.Count; i++)
		{
			hasItem &= playerInventory.IsInInventory(items[i].resource.ToString(), (int)items[i].amount);
		}
		return hasItem;
	}

	private void Dismantle()
	{
		if (buildingsData == null) { return; }

		dynamic building = GetBuildingInfo(cellPositionByMouse);
		dynamic buildingData = buildings[building.type.ToString()];
		Vector2I coords = new Vector2I((int)building.coords[0], (int)building.coords[1]);
		System.Collections.Generic.Dictionary<string, int> leftovers = new();		

		// gives player cost items back
		for (int i = 0; i < buildingData.cost.Count; i++)
		{
			int leftover = playerInventory.PutToInventory(buildingData.cost[i].resource.ToString(), (int)buildingData.cost[i].amount);
			//GD.Print(building.cost[i].resource, leftover);
			if (leftover != 0)
			{
				if (leftovers.ContainsKey(buildingData.cost[i].resource.ToString()))
				{
					leftovers[buildingData.cost[i].resource.ToString()] += leftover;
				}
				else
				{
					leftovers.Add(buildingData.cost[i].resource.ToString(), leftover);
				}
			}
		}

		// give player item from slots
		if (building.buildingType.ToString() == "machine")
		{
			// goes through every input slot of machine
			for (int i = 0; i < building.inputSlots.Count; i++)
			{
				int leftover = playerInventory.PutToInventory(building.inputSlots[i].resource.ToString(), (int)building.inputSlots[i].amount);			
				//GD.Print(building.inputSlots[i].resource, leftover);
				if (leftover != 0)
				{
					if (leftovers.ContainsKey(building.inputSlots[i].resource.ToString()))
					{
						leftovers[building.inputSlots[i].resource.ToString()] += leftover;
					}
					else
					{
						leftovers.Add(building.inputSlots[i].resource.ToString(), leftover);
					}
				}
			}

			// goes through every output slot of machine
			for (int i = 0; i < building.outputSlots.Count; i++)
			{
				int leftover = playerInventory.PutToInventory(building.outputSlots[i].resource.ToString(), (int)building.outputSlots[i].amount);
				//GD.Print(building.outputSlots[i].resource, leftover);
				if (leftover != 0)
				{
					if (leftovers.ContainsKey(building.outputSlots[i].resource.ToString()))
					{
						leftovers[building.outputSlots[i].resource.ToString()] += leftover;
					}
					else
					{
						leftovers.Add(building.outputSlots[i].resource.ToString(), leftover);
					}
				}
			}
		}

		if (building.buildingType.ToString() == "storage")
		{
			for (int i = 0; i < (int)buildingData.slotsAmount; i++)
			{
				int leftover = playerInventory.PutToInventory(building.slots[i].resource.ToString(), (int)building.slots[i].amount);
				//GD.Print(building.slots[i].resource, leftover);
				if (leftover != 0)
				{
					if (leftovers.ContainsKey(building.slots[i].resource.ToString()))
					{
						leftovers[building.slots[i].resource.ToString()] += (int)building.slots[i].amount;
					}
					else
					{
						leftovers.Add(building.slots[i].resource.ToString(), (int)building.slots[i].amount);
					}
				}
			}
		}

		if (building.buildingType.ToString().Contains("belt") && building.item.ToString() != "")
		{
			Item item;
			string itemName = "";
			if (building.buildingType.ToString() == "belt")
			{
				itemName = $"{building.coords[0]}x{building.coords[1]}";
			}
			else if (building.buildingType.ToString() == "beltArm")
			{
				itemName = $"{building.coords[0] + building.nextPosition[0]}x{building.coords[1] + building.nextPosition[1]}";
				if (GetNodeOrNull<Item>(itemName) == null && building.item.ToString() != "")
				{
					itemName = $"{building.coords[0]}x{building.coords[1]}";
				}
			}
			item = GetNode<Item>(itemName);

			//item.PickUpItem();

			if (!item.PickUpItem())
			{
				if (leftovers.ContainsKey(item.itemType))
				{
					leftovers[item.itemType] += 1;
				}
				else
				{
					leftovers.Add(item.itemType, 1);
				}
				item.QueueFree();
			}
		}

		if (leftovers.Count != 0)
		{
			//GD.Print(leftovers.Count);
			CreateRemainsBox(leftovers);
		}

		// removes building's particles
		EmitSignal(SignalName.RemoveParticle, coords);
		EmitSignal(SignalName.RemoveAnimatedBuildingPart, coords, building.type.ToString());
		EmitSignal(SignalName.RemoveBuildingPart, coords, building.type.ToString());
		
		// destroys building
		buildingsInfo.Remove(building);
		EraseCell(1, coords);

		// destroys multi-tile building
		if ((bool)buildingData.hasAdditionalAtlasPosition)
		{
			for (int i = 0; i < buildingData.additionalAtlasPosition.Count; i++)
			{
				coords = new Vector2I((int)building.coords[0] + (int)buildingData.additionalAtlasPosition[i][0], (int)building.coords[1] + (int)buildingData.additionalAtlasPosition[i][1]);
				dynamic buildingPart = GetBuildingInfo(coords, true);
				buildingsInfo.Remove(buildingPart);
				EraseCell(1, coords);
			}
		}
	}

	public dynamic GetBuildingInfo(Vector2I cellPosition, bool getBuildingPart = false) 
	{
		foreach (var building in buildingsInfo)
		{
			if (building.coords[0] == cellPosition[0] && building.coords[1] == cellPosition[1])
			{
				if (building.buildingType.ToString() == "buildingPart" && !getBuildingPart)
				{
					return GetBuildingInfo(new Vector2I((int)building.parentBuilding[0], (int)building.parentBuilding[1]));
				}
				else
				{
					return building;
				}
			}
		}
		return null;
	}

	private static bool GroundResourceValidate(dynamic canBePlacedOn, string groundResourceName)
	{
		foreach(string resource in canBePlacedOn)
		{
			if (resource == groundResourceName)
			{
				return true;
			}
		}
		return false;
	}

	private bool BuildingSlotValidate(dynamic building, dynamic recipe)
	{
		for (int i = 0; i < building.inputSlots.Count; i++)
		{
			// checks if in 'inputSlot' is right item and its amount required to crafting
			if ((int)building.inputSlots[i].amount < (int)recipe.input[i].amount || 
				building.inputSlots[i].resource.ToString() != recipe.input[i].name.ToString())
			{
				return false;
			}
		}

		for (int i = 0; i < building.outputSlots.Count; i++)
		{
			// checks if in 'outputSlot' is right item and is enough space required to crafting
			string resource = building.outputSlots[i].resource.ToString();
			if (items[resource].maxStackSize - (int)building.outputSlots[i].amount < (int)recipe.output[i].amount || 
				building.outputSlots[i].resource.ToString() != recipe.output[i].name.ToString())
			{
				return false;
			}
		}

		return true;
	}

	public void RemoveItemsFromSlot(Vector2I coords, int itemAmount, string slotType, int slotIndex)
	{
		dynamic building = GetBuildingInfo(coords);
		if (building.buildingType.ToString() == "machine")
		{
			slotType = slotType.ToLower();
			building[slotType + "Slots"][slotIndex].amount = building[slotType + "Slots"][slotIndex].amount - itemAmount;

			if (slotType == "input" && (int)building[slotType + "Slots"][slotIndex].amount == 0)
			{
				building.productionProgress = 0;
			}

			//building[slotType + "Slots"][slotIndex].amount = 0;
		}
		else
		{
			building.slots[slotIndex].amount = building.slots[slotIndex].amount - itemAmount;

			if ((int)building.slots[slotIndex].amount == 0)
			{
				building.slots[slotIndex].resource = "";
			}
			
		}
	}

	public void PutItemsToSlot(Vector2I coords, int itemAmount, string itemType, string slotType, int slotIndex)
	{
		dynamic building = GetBuildingInfo(coords);
		if (building.buildingType.ToString() == "machine")
		{
			slotType = slotType.ToLower();
			dynamic slot = building[slotType + "Slots"][slotIndex];
			dynamic item = items[slot.resource.ToString()];

			if (slot.amount + itemAmount > item.maxStackSize)
			{
				slot.amount = item.maxStackSize;
			}
			else
			{
				slot.amount += itemAmount;
			}	
		}
		else
		{
			building.slots[slotIndex].resource = itemType;
			building.slots[slotIndex].amount = itemAmount;
		}
		//GD.Print(building);
	}

	private void CreateItem(Vector2 coords, Vector2I destination, string name, float speed = 0, string id = "", Vector2I? parentBuilding = null)
	{
		Item item = (Item)GD.Load<PackedScene>("res://Scenes/Game/World/Item/Item.tscn").Instantiate();
		if (id == "")
		{
			item.destination = destination * 64;
			item.speed = 64 / (60 / speed);

			if (parentBuilding != null)
			{
				item.parentBuilding = parentBuilding;
				Vector2I parentCoords = (Vector2I)parentBuilding;
				item.Name = $"{parentCoords[0]}x{parentCoords[1]}";
				item.GetNode<Sprite2D>("ItemHolder").Show();
				item.ZIndex = 1;
			}
			else
			{
				item.Name = $"{destination[0]}x{destination[1]}";
			}
		}
		else
		{
			item.Name = id;
			item.onGround = true;
		}

		item.Position = coords * 64;
		AddChild(item); 
		item.UpdateItem(name);
	}

	public void CreateRemainsBox(System.Collections.Generic.Dictionary<string, int> itemsDict)
	{
		List<LeftoversSlot> slots = new();
		Vector2 playerPosition = GetNode<Player>("../Player").Position;
		Leftovers leftovers = (Leftovers)GD.Load<PackedScene>("res://Scenes/Game/World/Leftovers/Leftovers.tscn").Instantiate();
		leftovers.GlobalPosition = playerPosition;

		foreach (var item in itemsDict)
		{
			int amount = item.Value;
			while (amount > (int)items[item.Key].maxStackSize)
			{
				LeftoversSlot additionalSlot = new (item.Key, (int)items[item.Key].maxStackSize);
				amount -= (int)items[item.Key].maxStackSize;

				slots.Add(additionalSlot);
			}
			LeftoversSlot slot = new (item.Key, amount);
			slots.Add(slot);
		}

		/*for (int i = 0; i < slots.Count; i++)
		{
			GD.Print("itemType: ", slots[i].itemType, " amount: ", slots[i].itemAmount);
		}*/

		leftovers.items = slots;
		AddChild(leftovers);
		//GD.Print("Created Remains Box");
	}

	private bool CanTBeltTransfer(dynamic building)
	{
		dynamic nextBuilding = GetBuildingInfo(new Vector2I((int)building.coords[0] + (int)building.nextPosition[0], (int)building.coords[1] + (int)building.nextPosition[1]));

		// checks if 'nextBuilding' exists ans that it's belt
		if (nextBuilding == null || nextBuilding.buildingType.ToString() != "belt") { return false; }

		// check if the 'nextBuilding' is rotated opposite to current one
		bool isOpposite = (int)building.nextPosition[0] * -1 == (int)nextBuilding.nextPosition[0] && (int)building.nextPosition[1] * -1 == (int)nextBuilding.nextPosition[1];
		
		return !isOpposite && (double)building.moveProgress >= 1 && nextBuilding.item.ToString() == "";
	}

	private bool CanArmTransfer(dynamic building)
	{
		dynamic previousBuilding = GetBuildingInfo(new Vector2I((int)building.coords[0] + (int)building.previousPosition[0], (int)building.coords[1] + (int)building.previousPosition[1]));
		dynamic nextBuilding = GetBuildingInfo(new Vector2I((int)building.coords[0] + (int)building.nextPosition[0], (int)building.coords[1] + (int)building.nextPosition[1]));

		bool hasItem = false;
		bool hasSpace = false;
		string previousItem = null;

		if ((double)building.moveProgress == 0)
		{
			// checks if both 'previousBuilding' and 'nextBuilding' exist
			if (previousBuilding == null || nextBuilding == null) { return false; }
			
			// if 'previousBuilding' has any item
			if (previousBuilding.buildingType.ToString() == "machine") // machine
			{
				hasItem = (int)previousBuilding.outputSlots[0].amount != 0;
				if (hasItem) { previousItem = previousBuilding.outputSlots[0].resource.ToString(); }
			}
			else if (previousBuilding.buildingType.ToString() == "belt") // belt
			{
				hasItem = previousBuilding.buildingType.ToString() != "beltArm" && (double)previousBuilding.moveProgress >= 1;
				if (hasItem) { previousItem = previousBuilding.item.ToString(); }
			}
			else if (previousBuilding.buildingType.ToString() == "storage")
			{
				dynamic buildingData = buildings[previousBuilding.type.ToString()];
				hasItem = (bool)buildingData.beltArmInteraction && HasStorageAnyItem(previousBuilding);
				if (hasItem) { previousItem = FindItemInStorage(previousBuilding); }
			}

			// if 'nextBuilding' has space for item
			if (nextBuilding.buildingType.ToString() == "machine") // machine
			{
				if (nextBuilding.recipe.ToString() == "none") { return false; }
				
				dynamic recipe = recipes[nextBuilding.recipe.ToString()];
				for (int i = 0; i < recipe.input.Count; i++)
				{
					if ((int)nextBuilding.inputSlots[i].amount == 0)
					{
						hasSpace = previousItem == recipe.name.ToString();
					}
					else
					{
						hasSpace = (int)nextBuilding.inputSlots[i].amount < (int)items[nextBuilding.inputSlots[i].resource.ToString()].maxStackSize && previousItem == nextBuilding.inputSlots[i].resource.ToString();
					}

					if (hasSpace) { break; }
				}
			}
			else if (nextBuilding.buildingType.ToString() == "belt") // belt
			{
				hasSpace = nextBuilding.buildingType.ToString() != "beltArm" && nextBuilding.item.ToString() == "";
			}
			else if (nextBuilding.buildingType.ToString() == "storage") // storage
			{
				dynamic buildingData = buildings[nextBuilding.type.ToString()];
				hasSpace = (bool)buildingData.beltArmInteraction && HasStorageSpace(nextBuilding, previousItem);
			}
			
			return hasItem && hasSpace;
		}

		return false;
	}

	private bool HasStorageAnyItem(dynamic building)
	{
		for (int i = building.slots.Count - 1; i >= 0; i--)
		{
			if (building.slots[i].resource.ToString() != "")
			{
				return true;
			}
		}

		return false;
	}

	private string FindItemInStorage(dynamic building)
	{
		for (int i = building.slots.Count - 1; i >= 0; i--)
		{
			if (building.slots[i].resource.ToString() != "")
			{
				return building.slots[i].resource.ToString();
			}
		}

		return null;
	}

	private bool HasStorageSpace(dynamic building, string item)
	{
		for (int i = 0; i < building.slots.Count; i++)
		{
			// if slot is empty or resource in slot isn't at its stack size
			if (building.slots[i].resource.ToString() == "" || item == building.slots[i].resource.ToString() && (int)building.slots[i].amount < (int)items[building.slots[i].resource.ToString()].maxStackSize)
			{
				return true;
			}
		}
		return false;
	}

	public void ChangeRecipe(string recipe, Vector2I coords)
	{
		dynamic building = GetBuildingInfo(coords);
		dynamic currentRecipe = recipes[recipe];

		if (building.recipe == currentRecipe) { return; }

		building.recipe = recipe;
		building.productionProgress = 0;

		for (int i = 0; i < currentRecipe.input.Count; i++)
		{
			if ((int)building.inputSlots[i].amount == 0)
			{
				building.inputSlots[i].resource = currentRecipe.input[i].name;
			}
		}

		for (int i = 0; i < currentRecipe.output.Count; i++)
		{
			if ((int)building.outputSlots[i].amount == 0)
			{
				building.outputSlots[i].resource = currentRecipe.output[i].name;
			}

		}
	}
}
