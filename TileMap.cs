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

public partial class TileMap : Godot.TileMap
{
	[Signal]
	public delegate void ResourcesUpdatedEventHandler();

	[Signal]
	public delegate void UpdateResourceInfoEventHandler(string resourceName);

	[Signal]
	public delegate void ToggleInventoryEventHandler(bool InvenotryToggle, string info);

	[Signal]
	public delegate void UpdateBuildingProgressEventHandler(string info);


	public bool BUILDINGMODE = false;
	public bool DISMANTLEMODE = false;
	public bool UITOGGLE = false;

	Vector2I previousCellPositon = new (0, 0);

	string groundResourceName;
	TileData buildingsData;
	Vector2I cellPostionByMouse;

	// setting default grid curor parameters
	int tileMapId = 100;
	int tileMapLayer = 3;
	Vector2I atlasPosition = new (0, 0);
	string[] buildingsCoords = new string[] {};
	public List<dynamic> buildingsInfo = new();
	public string selectedBuilding;
	int buildingRotation = 0;
	dynamic resourcesHervestedByHand;

	int resourceAmount = 100000;
	dynamic buildings;
	dynamic items;
	dynamic groundResources;
	dynamic recipes;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// hide cursor
		//Input.MouseMode = Input.MouseModeEnum.Hidden;
		
		EmitSignal(SignalName.ResourcesUpdated, resourceAmount);

		LoadFile load = new();

		// loads 'buildigns.json' file and parses in to dynamic object
		buildings = load.LoadJson("buildings.json");
		resourcesHervestedByHand = buildings.handHarvest;

		items = load.LoadJson("items.json");
		groundResources = load.LoadJson("groundResources.json");

		recipes = load.LoadJson("recipes.json");

		// this generates the world
		GenerateWorld generateWorld = GetNode<GenerateWorld>("/root/main/GenerateWorld");
		Vector2I mapSize = new Vector2I(500, 500);
		generateWorld.GenerateResource(mapSize, "Grass", true);
		generateWorld.GenerateResource(mapSize, "IronOre");
		generateWorld.GenerateResource(mapSize, "CoalOre");
		generateWorld.GenerateResource(mapSize, "CopperOre");

		//UITOGGLE = GetNode<UIToggle>("/root/main/UI/UIToggle").toggle;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		PauseMenu pauseMenu = GetNode<PauseMenu>("/root/main/UI/PauseMenu");
		pauseMenu.CanPause = !UITOGGLE && !BUILDINGMODE;


		// getting mouse position with TileMap coordinates
		cellPostionByMouse = GetMousePosition();

		// getting tileMap data by mouse coordinates that hovers over TileMap cell
		TileData groundData = GetCellTileData(0, cellPostionByMouse);
		buildingsData = GetCellTileData(1, cellPostionByMouse);

		if (groundData == null) { return; }

		groundResourceName = (string)groundData.GetCustomData("resourceName");

		string worldInfo;

		/*if (BUILDINGMODE && Input.IsActionJustPressed("Back"))
		{
			ToggleBuildMode();
		}*/

		
		// if any inventory is oppend any of the actions bellow won't work
		if (UITOGGLE) { return; }
		
		if (buildingsData != null && GetBuildingInfo(cellPostionByMouse).buildingType.ToString() == "machine")
		{
			dynamic buildingDisplayInfo = GetBuildingInfo(cellPostionByMouse);

			worldInfo = $"Building: {buildingDisplayInfo.name} \nProgress: {(int)(buildingDisplayInfo.productionProgress * 100)}%";	
		}
		else if (buildingsData != null && (GetBuildingInfo(cellPostionByMouse).buildingType.ToString() == "belt" || GetBuildingInfo(cellPostionByMouse).buildingType.ToString() == "beltArm"))
		{
			dynamic buildingDisplayInfo = GetBuildingInfo(cellPostionByMouse);

			worldInfo = $"Building: {buildingDisplayInfo.name} \nProgress: {(int)(buildingDisplayInfo.moveProgress * 100)}% \nItem: {buildingDisplayInfo.item}";	
		}
		else
		{
			worldInfo = $"Resource: {groundResources[groundResourceName].name}";
		}

		worldInfo = $"Coordinates: {cellPostionByMouse[0]}, {cellPostionByMouse[1]}\n{worldInfo}";
		
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
			if(Input.IsActionJustPressed("Use"))
			{
				// farming resources
				FarmResources(GetBuildingInfo(cellPostionByMouse));
			}
						
			if (Input.IsActionJustPressed("Interact"))
			{
				dynamic buildingDisplayInfo = GetBuildingInfo(cellPostionByMouse);

				if (!UITOGGLE && buildingsData != null)
				{
					UITOGGLE = true;	
				}
				else
				{
					UITOGGLE = false;
				}
				
				EmitSignal(SignalName.ToggleInventory, UITOGGLE, Newtonsoft.Json.JsonConvert.SerializeObject(buildingDisplayInfo));
			}
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
			switch(buildingsInfo[i].buildingType.ToString())
			{
				case "machine":
					if (buildingsInfo[i].recipe.ToString() == "none") { continue; } // if no recipe is selected, skips this building

					dynamic recipe = recipes[buildingsInfo[i].recipe.ToString()];
					// GD.Print(buildingsInfo[i]);
					if (BuildingSlotValidate(buildingsInfo[i], recipe))
					{
						buildingsInfo[i].productionProgress += (double)recipe.cyclesPerMinute / 60 * delta;

						if (buildingsInfo[i].productionProgress >= 1) // if 'productionProgress' is full items will be added and removed acording to machine's recipe
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

					// if inventory is oppened, data will be sent to the inventory to show on screeen
					if (UITOGGLE)
					{
						EmitSignal(SignalName.UpdateBuildingProgress, Newtonsoft.Json.JsonConvert.SerializeObject(buildingsInfo[i]));
					}
					break;
				
				case "belt":
					// gets the next belt
					nextCoords = new Vector2I((int)buildingsInfo[i].coords[0] + (int)buildingsInfo[i].nextPosition[0], (int)buildingsInfo[i].coords[1] + (int)buildingsInfo[i].nextPosition[1]);
					nextBuilding = GetBuildingInfo(nextCoords);
						
					// moves item on the belt
					if (buildingsInfo[i].item.ToString() != "" && (double)buildingsInfo[i].moveProgress < 1 && nextBuilding != null)
					{
						buildingsInfo[i].moveProgress += (double)buildingsInfo[i].speed / 60 * delta;
					}

					if ((double)buildingsInfo[i].moveProgress > 1) { buildingsInfo[i].moveProgress = 1; }
					
					// moves item to the next belt
					if (CanTBeltTransfer(buildingsInfo[i]))
					{
						itemName = $"{buildingsInfo[i].coords[0]}x{buildingsInfo[i].coords[1]}";
						item = GetNode<Item>(itemName);

						item.destination = nextCoords * 64;
						item.Name = $"{nextCoords[0]}x{nextCoords[1]}";
						item.speed = 64 / (60 / (int)buildingsInfo[i].speed);

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
						buildingsInfo[i].moveProgress += (double)buildingsInfo[i].speed / 60 * delta;
					}

					if ((double)buildingsInfo[i].moveProgress > 1) { buildingsInfo[i].moveProgress = 1; }
					
					if ((double)buildingsInfo[i].moveProgress == 0 && CanArmTransfer(buildingsInfo[i]))
					{
						if (previousBuilding.buildingType.ToString() == "machine") // machine
						{
							buildingsInfo[i].item = previousBuilding.outputSlots[0].resource;
							previousBuilding.outputSlots[0].amount -= 1;
							CreateItem(previousCoords, nextCoords, buildingsInfo[i].item.ToString(), (int)buildingsInfo[i].speed * 2);
						}
						else // belt
						{
							buildingsInfo[i].item = previousBuilding.item;
							previousBuilding.item = "";
							previousBuilding.moveProgress = 0;

							itemName = $"{previousCoords[0]}x{previousCoords[1]}";
							item = GetNode<Item>(itemName);

							item.destination = nextCoords * 64;
							item.speed = 64 / (60 / (int)buildingsInfo[i].speed) * 2;

							item.Name = $"{nextCoords[0]}x{nextCoords[1]}";

						}

						if (nextBuilding.buildingType.ToString() == "belt")
						{
							nextBuilding.item = buildingsInfo[i].item;
						}
					}
					else if ((double)buildingsInfo[i].moveProgress >= 1)
					{

						if (nextBuilding.buildingType.ToString() == "machine") // machine
						{
							itemName = $"{nextCoords[0]}x{nextCoords[1]}";
							item = GetNode<Item>(itemName);
							nextBuilding.inputSlots[0].amount += 1;

							item.QueueFree();
						}

						buildingsInfo[i].item = "";
						buildingsInfo[i].moveProgress = 0;
					}
					
				break;
			}
		}
	}

	public Vector2I GetMousePosition()
	{
		var mousePosition = GetGlobalMousePosition();
		Vector2I cellPostionByMouse = new ((int)(mousePosition[0] / (4 * 16)), (int)(mousePosition[1]  / (4 * 16)));

		if (mousePosition[0] < 0)
		{
			cellPostionByMouse = new Vector2I((int)(cellPostionByMouse[0] - 1), (int)(cellPostionByMouse[1]));
		}

		if (mousePosition[1] < 0)
		{
			cellPostionByMouse = new Vector2I((int)(cellPostionByMouse[0]), (int)(cellPostionByMouse[1] - 1));
		}

		return cellPostionByMouse;
	}

	private void CursorTexture()
	{
		ClearLayer(tileMapLayer);
		
		if (BUILDINGMODE)
		{
			Vector2I atlasCoords = new Vector2I((int)buildings[selectedBuilding].atlasCoords[0] + buildingRotation, (int)buildings[selectedBuilding].atlasCoords[1]);
			SetCell(tileMapLayer, cellPostionByMouse, tileMapId, atlasCoords);

			if ((bool)buildings[selectedBuilding].hasAdditionalAtlasPosition)
			{
				for (int i = 0; i < buildings[selectedBuilding].additionalAtlasPosition.Count; i++)
					{
						Vector2I additionalAtlasPosition = new Vector2I((int)buildings[selectedBuilding].additionalAtlasPosition[i][0], (int)buildings[selectedBuilding].additionalAtlasPosition[i][1]);
						SetCell(tileMapLayer, cellPostionByMouse + additionalAtlasPosition, tileMapId, atlasCoords + additionalAtlasPosition);
					}
			}
		}
		else {
			SetCell(tileMapLayer, cellPostionByMouse, tileMapId, atlasPosition);
		}

	}

	private void FarmResources(dynamic buildingDisplayInfo)
	{
		if (GroundResourceValidate(resourcesHervestedByHand.canBeUsedOn, groundResourceName) && buildingsData == null)
		{
			resourceAmount++;
			EmitSignal(SignalName.ResourcesUpdated, resourceAmount);
		}

		if (buildingsData != null && (Input.IsActionPressed("TakeAll") || Input.IsActionJustPressed("Use"))) {
			resourceAmount += (int)buildingDisplayInfo.outputSlots[0].amount;
			buildingDisplayInfo.outputSlots[0].amount = 0;
			EmitSignal(SignalName.ResourcesUpdated, resourceAmount);
		}
			
	}

	private void RotateBuilding()
	{
		buildingRotation = (buildingRotation + 1) % (int)buildings[selectedBuilding].rotationAmount;
		//GD.Print(buildingRotation);
	}

	public void ToggleBuildMode()
	{
		BUILDINGMODE = !BUILDINGMODE;
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
	}

	private void ToggleDismantleMode()
	{
		DISMANTLEMODE = !DISMANTLEMODE;
		BUILDINGMODE = false;

		ClearLayer(tileMapLayer);
		tileMapLayer = 3;
		tileMapId = 100;
		atlasPosition = new (0, 0);	

		if (DISMANTLEMODE)
		{
			// draws cross crosshair to TileMap
			atlasPosition = new (1, 0);
		}
	}

	private void Build()
	{
		if (GroundResourceValidate(buildings[selectedBuilding].canBePlacedOn, groundResourceName) && buildingsData == null /*&& resourceAmount >= (int)buildings[selectedBuilding].cost*/)
		{
			SetCell(1, cellPostionByMouse, 1, new((int)buildings[selectedBuilding].atlasCoords[0] + buildingRotation, (int)buildings[selectedBuilding].atlasCoords[1]));
			/*resourceAmount -= (int)buildings[selectedBuilding].cost;
			EmitSignal(SignalName.ResourcesUpdated, resourceAmount);*/

			string buildingsJson = Newtonsoft.Json.JsonConvert.SerializeObject(buildings);
			dynamic building = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(buildingsJson);
			building = building[selectedBuilding];
			building.coords[0] = cellPostionByMouse[0];
			building.coords[1] = cellPostionByMouse[1];
			
			if  (building.type.ToString() == "drill")
			{
				building.outputSlots[0].resource = groundResourceName;
				building.recipe = groundResourceName;
			}

			if (building.buildingType.ToString() == "belt" || building.buildingType.ToString() == "beltArm")
			{
				building.rotation = buildingRotation;
				Vector2I nextPosition = new Vector2I(0, 0);
				
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
			}

			if (building.buildingType.ToString() == "beltArm")
			{
				building.previousPosition[0] = -1 * (int)building.nextPosition[0];
				building.previousPosition[1] = -1 * (int)building.nextPosition[1];
			}

			if((bool)building.hasAdditionalAtlasPosition)
			{
				for (int i = 0; i < building.additionalAtlasPosition.Count; i++)
				{
					Vector2I atlasCoords = new ((int)building.atlasCoords[0], (int)building.atlasCoords[1]);
					Vector2I additionalAtlasPosition = new ((int)building.additionalAtlasPosition[i][0], (int)building.additionalAtlasPosition[i][1]);
					
					dynamic buildingPart = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(buildingsJson);;
					buildingPart = buildingPart.buildingPart;

					buildingPart.coords[0] = cellPostionByMouse[0] + additionalAtlasPosition[0];
					buildingPart.coords[1] = cellPostionByMouse[1] + additionalAtlasPosition[1];
					buildingPart.parentBuilding[0] = building.coords[0];
					buildingPart.parentBuilding[1] = building.coords[1];

					SetCell(1, cellPostionByMouse + additionalAtlasPosition, 1, atlasCoords + additionalAtlasPosition);
					buildingsInfo.Add(buildingPart);
				}
			}

			buildingsInfo.Add(building);
			buildings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(buildingsJson);
		}
	}

	private void Dismantle()
	{
		if (buildingsData == null) { return; }

		dynamic building = GetBuildingInfo(cellPostionByMouse);
		Vector2I coords = new Vector2I((int)building.coords[0], (int)building.coords[1]);

		PlayerInventory playerInventory = GetNode<PlayerInventory>("/root/main/UI/Inventories/InventoryGrid/PlayerInventory");
		List<InventorySlot> leftovers = new();		

		for (int i = 0; i < building.cost.Count; i++)
		{
			int leftover = playerInventory.PutToInventory(building.cost[i].resource.ToString(), (int)building.cost[i].amount);
			if (leftover != 0)
			{
				InventorySlot slot = new();
				slot.itemType = building.cost[i].resource.ToString();
				GetNode<Label>("ResourceName").Text = leftover.ToString();
				leftovers.Add(slot);
			}
		}

		if (building.buildingType.ToString() == "machine")
		{
			// goes through every input slot of machine
			for (int i = 0; i < building.inputSlots.Count; i++)
			{
				int leftover = playerInventory.PutToInventory(building.inputSlots[i].resource.ToString(), (int)building.inputSlots[i].amount);
				if (leftover != 0)
				{
					InventorySlot slot = new();
					slot.itemType = building.inputSlots[i].resource.ToString();
					GetNode<Label>("ResourceName").Text = leftover.ToString();
					leftovers.Add(slot);
				}
			}

			// goes through every output slot of machine
			for (int i = 0; i < building.outputSlots.Count; i++)
			{
				int leftover = playerInventory.PutToInventory(building.outputSlots[i].resource.ToString(), (int)building.outputSlots[i].amount);
				if (leftover != 0)
				{
					InventorySlot slot = new();
					slot.itemType = building.outputSlots[i].resource.ToString();
					GetNode<Label>("ResourceName").Text = leftover.ToString();
					leftovers.Add(slot);
				}
			}
		}

		if (building.buildingType.ToString().Contains("belt") && building.item.ToString() != "")
		{
			string itemName = $"{building.coords[0]}x{building.coords[1]}";
			Item item = GetNode<Item>(itemName);

			item.PickUpItem();

			if (!item.PickUpItem())
			{
				item.destination = item.Position;
			}
		}
		
		// destroys building
		buildingsInfo.Remove(building);
		EraseCell(1, coords);

		// ďestroys multi-tile building
		if ((bool)building.hasAdditionalAtlasPosition)
		{
			for (int i = 0; i < building.additionalAtlasPosition.Count; i++)
			{
				coords = new Vector2I((int)building.coords[0] + (int)building.additionalAtlasPosition[i][0], (int)building.coords[1] + (int)building.additionalAtlasPosition[i][1]);
				dynamic buildingPart = GetBuildingInfo(coords);
				buildingsInfo.Remove(building);
				EraseCell(1, coords);
			}
		}
	}

	public dynamic GetBuildingInfo(Vector2 cellPostion) 
	{
		foreach (var building in buildingsInfo)
		{
			if (building.coords[0] == cellPostion[0] && building.coords[1] == cellPostion[1])
			{
				if (building.buildingType.ToString() == "buildingPart")
				{
					return GetBuildingInfo(new Vector2((int)building.parentBuilding[0], (int)building.parentBuilding[1]));
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
			// checks if in 'inputSlot' is right item and its amout required to crafting
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

	public void RemoveItemFromSlot(Vector2I coords, string slotType, int slotIndex)
	{
		dynamic building = GetBuildingInfo(coords);
		slotType = slotType.ToLower();

		if (slotType == "input")
		{
			building.productionProgress = 0;
		}

		resourceAmount += (int)building[slotType + "Slots"][slotIndex].amount;
		building[slotType + "Slots"][slotIndex].amount = 0;
		EmitSignal(SignalName.ResourcesUpdated, resourceAmount);
	}

	public void PutItemToSlot(Vector2I coords, int itemAmount, int slotIndex, string slotType)
	{
		dynamic building = GetBuildingInfo(coords);
		slotType = slotType.ToLower();

		building[slotType + "Slots"][slotIndex].amount = itemAmount;
	}

	private void CreateItem(Vector2I coords, Vector2I destination, string name, int speed = 0, string id = "")
	{
		Item item = (Item)GD.Load<PackedScene>("res://Scenes/Game/World/Item/Item.tscn").Instantiate();

		if (id == "")
		{
			item.Name = $"{destination[0]}x{destination[1]}";
			item.destination = destination * 64;
			item.speed = 64 / (60 / speed);
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

		bool hasItem;
		bool hasSpace;
		string previousItem = null;

		if ((double)building.moveProgress == 0)
		{
			// checks if both 'previousBuilding' and 'nextBuilding' exist
			if (previousBuilding == null || nextBuilding == null) { return false; }
			
			if (previousBuilding.buildingType.ToString() == "machine") // machine
			{
				hasItem = (int)previousBuilding.outputSlots[0].amount != 0;
				if (hasItem) { previousItem = previousBuilding.outputSlots[0].resource.ToString(); }
			}
			else // belt
			{
				hasItem = previousBuilding.buildingType.ToString() != "beltArm" && (double)previousBuilding.moveProgress >= 1;
				if (hasItem) { previousItem = previousBuilding.item.ToString(); }
			}

			if (nextBuilding.buildingType.ToString() == "machine") // machine
			{
				if (nextBuilding.recipe.ToString() == "none") { return false; }
				
				if ((int)nextBuilding.inputSlots[0].amount == 0)
				{
					hasSpace = previousItem == recipes[nextBuilding.recipe.ToString()].input[0].name.ToString();
				}
				else
				{
					hasSpace = (int)nextBuilding.inputSlots[0].amount < (int)items[nextBuilding.inputSlots[0].resource.ToString()].maxStackSize && previousItem == nextBuilding.inputSlots[0].resource.ToString();
				}
			}
			else // belt
			{
				hasSpace = nextBuilding.buildingType.ToString() != "beltArm" && nextBuilding.item.ToString() == "";
			}
			
			return hasItem && hasSpace;
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
