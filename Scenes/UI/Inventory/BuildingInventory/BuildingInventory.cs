using Godot;
using Godot.Collections;
using System;
using System.Data;
using System.Diagnostics.SymbolStore;

public partial class BuildingInventory : Control
{
	[Signal]
	public delegate void DisableInventoryEventHandler();

	private bool INVENTORYOPPENED = true;
	private dynamic recipes;
	public string INVENTORYTYPE = "machine";
	public dynamic buildingInfo;
	World tileMap;
	private Label building;
	private Label resources;
	public Label resourceProduction;
	ProgressBar productionProgress;
	Vector2I coordinates;
	LoadFile load = new();
	dynamic items;
	dynamic buildings;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		recipes = load.LoadJson("recipes.json");
		items = load.LoadJson("items.json");
		buildings = load.LoadJson("buildings.json");

		tileMap = GetNode<World>("/root/main/World/TileMap");
		tileMap.ToggleInventory += ToggleInventory;

		productionProgress = GetNode<ProgressBar>("TabContainer/Building/ProductionProgress");
		resourceProduction = GetNode<Label>("TabContainer/Building/Production");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ToggleInventory(bool TOGGLEINGINVENTORY, dynamic _buildingInfo, Leftovers leftovers = null)
	{
		InventorySlot slot;

		if (!TOGGLEINGINVENTORY) 
		{
			tileMap.UITOGGLE = false;

			Array<Node> inputSlots = GetTree().GetNodesInGroup("InputSlots");
			Array<Node> outputSlots = GetTree().GetNodesInGroup("OutputSlots");
			Array<Node> singleOutputSlots = GetTree().GetNodesInGroup("SingleSlots");
			Array<Node> storageSlots = GetTree().GetNodesInGroup("StorageSlots");

			for (int i = 0; i < inputSlots.Count; i++)
			{
				slot = (InventorySlot)inputSlots[i];
				slot.Hide();
				slot.UpdateSlotTexture("");
				slot.resourceAmount.Text = "";
			}

			for (int i = 0; i < outputSlots.Count; i++)
			{
				slot = (InventorySlot)outputSlots[i];
				slot.Hide();
				slot.UpdateSlotTexture("");
				slot.resourceAmount.Text = "";
			}

			for (int i = 0; i < singleOutputSlots.Count; i++)
			{
				slot = (InventorySlot)singleOutputSlots[i];
				slot.Hide();
				slot.UpdateSlotTexture("");
				slot.resourceAmount.Text = "";
			}
			
			for (int i = 0; i < storageSlots.Count; i++)
			{
				main.FindNodeByNameInGroup(storageSlots, $"StorageSlot{i}").QueueFree();
			}
				
			
			Array<Node> remainsSlots = GetTree().GetNodesInGroup("LeftoversSlots");
			for (int i = 0; i < remainsSlots.Count; i++)
			{
				slot = (InventorySlot)remainsSlots[i];
				int amount = 0;

				if (slot.itemType != "")
				{
					amount = int.Parse(slot.resourceAmount.Text);
				}

				leftovers.items[i].itemType = slot.itemType;
				leftovers.items[i].itemAmount = amount;
				main.FindNodeByNameInGroup(remainsSlots, $"LeftoversSlot{i}").QueueFree();	
			}

			if (leftovers != null)
			{
				leftovers.RemoveEmptySlots();
			}

			productionProgress.Value = 0;
			this.Hide();
			return; 
		}

		buildingInfo = _buildingInfo;
		TabContainer tabContainer = GetNode<TabContainer>("TabContainer");
		Label buildingName = GetNode<Label>("TabContainer/Building/Name");
		
		if (leftovers != null)
		{	
			tabContainer.TabsVisible = false;
			tabContainer.CurrentTab = 1;
			buildingName.Text = "Leftovers";

			productionProgress.Hide();
			resourceProduction.Hide();

			for (int i = 0; i < leftovers.items.Count; i++)
			{
				InventorySlot newSlot = (InventorySlot)GD.Load<PackedScene>("res://Scenes/UI/Inventory/InventorySlot.tscn").Instantiate();
				newSlot.Name = $"LeftoversSlot{i}";
				newSlot.AddToGroup("LeftoversSlots");
				newSlot.itemType =  leftovers.items[i].itemType;

				newSlot.GetNode<Label>("ResourceAmount").Text =  leftovers.items[i].itemAmount.ToString();
				
				GetNode<FlowContainer>("TabContainer/Building/Slots/StorageSlots/FlowContainer").AddChild(newSlot);
				newSlot.UpdateSlotTexture(newSlot.itemType);
			}

			GetNode<ScrollContainer>("TabContainer/Building/Slots/StorageSlots").Show();
			Show();
			return;
		}
		//GD.Print(buildingInfo);

		dynamic buildingData = buildings[buildingInfo.type.ToString()];
		coordinates = new ((int)buildingInfo.coords[0], (int)buildingInfo.coords[1]);
		switch (buildingInfo.buildingType.ToString())
		{
			case "machine":
				if ((bool)buildingData.canChooseRecipe)
				{
					tabContainer.TabsVisible = true;
				}
				else
				{
					tabContainer.TabsVisible = false;
				}

				GetNode<ScrollContainer>("TabContainer/Building/Slots/StorageSlots").Hide();
				productionProgress.Show();
				resourceProduction.Show();
					
				buildingName.Text = buildingData.name;
				dynamic recipe = recipes[buildingInfo.recipe.ToString()];
				resourceProduction.Text = "Recipe: none";

				if (!buildingInfo.type.ToString().Contains("Drill"))
				{
					TabContainer recipesTab = GetNode<TabContainer>("TabContainer/Recipes");
					switch (buildingInfo.type.ToString())
					{
						case "smelter":
							recipesTab.CurrentTab = 0;
							break;

						case "constructor":
							recipesTab.CurrentTab = 1;
							break;

						case "assembler":
							recipesTab.CurrentTab = 2;
							break;

						default:
							GD.PrintErr("Unknown building");
							break;
					}
				}

				if (buildingInfo.recipe.ToString() == "none")
				{
					tabContainer.CurrentTab = 0;
				}
				else
				{
					tabContainer.CurrentTab = 1;

					resourceProduction.Text = "Recipe: " + recipe.name.ToString();

					if (buildingInfo.type.ToString().Contains("Drill"))
					{
						slot = (InventorySlot)main.FindNodeByNameInGroup(GetTree().GetNodesInGroup("SingleSlots"), "DrillOutputSlot");
						slot.buildingCoordinates = coordinates;
						slot.itemType = buildingInfo.outputSlots[0].resource.ToString();
						slot.inventoryType = INVENTORYTYPE;
						slot.GetNode<Label>("Produce").Text = $"{recipe.output[0].amount}x {items[recipe.output[0].name.ToString()].name}";
						slot.GetNode<Label>("Rate").Text = $"{buildingData.productionMultiplier * buildingInfo.tiles * recipe.cyclesPerMinute * recipe.output[0].amount} / min";
						slot.UpdateSlotTexture(slot.itemType);
						slot.Show();
					}
					else
					{
						Array<Node> inputSlots = GetTree().GetNodesInGroup("InputSlots");
						Array<Node> outputSlots = GetTree().GetNodesInGroup("OutputSlots");

						for (int i = 0; i < recipe.input.Count; i++)
						{
							slot = (InventorySlot)inputSlots[i];
							slot.buildingCoordinates = coordinates;
							slot.itemType = buildingInfo.inputSlots[i].resource.ToString();
							slot.inventoryType = INVENTORYTYPE;
							slot.GetNode<Label>("Need").Text = $"{recipe.input[i].amount}x {items[recipe.input[i].name.ToString()].name}";
							slot.GetNode<Label>("Rate").Text = $"{recipe.cyclesPerMinute * recipe.input[i].amount} / min";
							slot.UpdateSlotTexture(slot.itemType);
							slot.Show();
						}

						for (int i = 0; i < recipe.output.Count; i++)
						{
							slot = (InventorySlot)outputSlots[i];
							slot.buildingCoordinates = coordinates;
							slot.itemType = buildingInfo.outputSlots[i].resource.ToString();
							slot.inventoryType = INVENTORYTYPE;
							slot.GetNode<Label>("Produce").Text = $"{recipe.output[i].amount.ToString()}x {items[recipe.output[i].name.ToString()].name.ToString()}";
							slot.GetNode<Label>("Rate").Text = $"{recipe.cyclesPerMinute * recipe.output[i].amount} / min";
							slot.UpdateSlotTexture(slot.itemType);
							slot.Show();
						}
					}
				}

				Show();
				break;

			case "storage":
				tabContainer.TabsVisible = false;
				tabContainer.CurrentTab = 1;

				buildingName.Text = buildingData.name.ToString();
				productionProgress.Hide();
				resourceProduction.Hide();
				
				for (int i = 0; i < (int)buildingInfo.slots.Count; i++)
				{
					InventorySlot newSlot = (InventorySlot)GD.Load<PackedScene>("res://Scenes/UI/Inventory/InventorySlot.tscn").Instantiate();
					newSlot.Name = $"StorageSlot{i}";
					newSlot.AddToGroup("StorageSlots");
					newSlot.inventoryType = "storage";
					newSlot.buildingCoordinates = coordinates;
					newSlot.itemType = buildingInfo.slots[i].resource.ToString();

					if ((int)buildingInfo.slots[i].amount != 0) { newSlot.GetNode<Label>("ResourceAmount").Text = buildingInfo.slots[i].amount.ToString(); }
					
					GetNode<FlowContainer>("TabContainer/Building/Slots/StorageSlots/FlowContainer").AddChild(newSlot);
					newSlot.UpdateSlotTexture(newSlot.itemType);
				}

				GetNode<ScrollContainer>("TabContainer/Building/Slots/StorageSlots").Show();
				Show();
				break;

			default:
				tileMap.UITOGGLE = false;
				break;
		}
	}

	public void UpdateInventory(dynamic building)
	{
		Vector2I coords = new Vector2I((int)building.coords[0], (int)building.coords[1]);
		if (coordinates != coords || GetTree().GetNodesInGroup("RemainsSlots").Count != 0) { return; }
		
		if (building.buildingType.ToString() == "machine") { productionProgress.Value = (double)building.productionProgress; }

		if (building.type.ToString().Contains("Drill"))
		{	
			InventorySlot slot = GetNode<InventorySlot>("TabContainer/Building/Slots/DrillOutputSlot");
			UpdateSlot(slot, building.outputSlots[0].resource.ToString(), (int)building.outputSlots[0].amount);
		}
		else if (building.buildingType.ToString() == "machine")
		{
			Array<Node> inputSlots = GetTree().GetNodesInGroup("InputSlots");
			Array<Node> outputSlots = GetTree().GetNodesInGroup("OutputSlots");

			for (int i = 0; i < recipes[building.recipe.ToString()].input.Count; i++)
			{
				InventorySlot slot = (InventorySlot)inputSlots[i];
				UpdateSlot(slot, building.inputSlots[i].resource.ToString(), (int)building.inputSlots[i].amount);
			}

			for (int i = 0; i < recipes[building.recipe.ToString()].output.Count; i++)
			{
				InventorySlot slot = (InventorySlot)outputSlots[i];
				UpdateSlot(slot, building.outputSlots[i].resource.ToString(), (int)building.outputSlots[i].amount);
			}
		}
		else if (building.buildingType.ToString() == "storage")
		{
			Array<Node> storageSlots = GetTree().GetNodesInGroup("StorageSlots");
			for (int i = 0; i < (int)building.slots.Count; i++)
			{
				InventorySlot slot = (InventorySlot)storageSlots[i];
				UpdateSlot(slot, building.slots[i].resource.ToString(), (int)building.slots[i].amount);
			}
		}
	}

	private void UpdateSlot(InventorySlot slot, string itemType, int itemAmount)
	{
		Label resourceAmount = GetNode<Label>(slot.GetPath() + "/ResourceAmount");

		if (itemAmount > 0)
		{		
			resourceAmount.Text = itemAmount.ToString();
			slot.itemType = itemType;
			slot.UpdateSlotTexture(itemType);
		}
		else
		{
			resourceAmount.Text = "";
			slot.UpdateSlotTexture("");
		}
	}
}
