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
	private Label building;
	private Label resources;
	public Label resourceProduction;
	ProgressBar productionProgress;
	Vector2I coordinates;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadFile load = new();
		recipes = load.LoadJson("recipes.json");

		TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
		tileMap.ToggleInventory += ToggleInventory;
		tileMap.UpdateBuildingProgress += UpdateInventory;

		productionProgress = GetNode<ProgressBar>("TabContainer/Building/ProductionProgress");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ToggleInventory(bool TOGGLEINGINVENTORY, string building)
	{
		if (!TOGGLEINGINVENTORY) 
		{
			Array<Node> inputSlots = GetTree().GetNodesInGroup("InputSlots");
			Array<Node> outputSlots = GetTree().GetNodesInGroup("OutputSlots");
			Array<Node> singleOutputSlots = GetTree().GetNodesInGroup("SingleSlots");

			for (int i = 0; i < inputSlots.Count; i++)
			{
				InventorySlot slot = (InventorySlot)inputSlots[i];
				slot.Hide();
				slot.UpdateSlotTexture("");
				slot.resourceAmount.Text = "";
			}

			for (int i = 0; i < outputSlots.Count; i++)
			{
				InventorySlot slot = (InventorySlot)outputSlots[i];
				slot.Hide();
				slot.UpdateSlotTexture("");
				slot.resourceAmount.Text = "";
			}

			for (int i = 0; i < singleOutputSlots.Count; i++)
			{
				InventorySlot slot = (InventorySlot)singleOutputSlots[i];
				slot.Hide();
				slot.UpdateSlotTexture("");
				slot.resourceAmount.Text = "";
			}
			
			productionProgress.Value = 0;
			this.Hide();
			return; 
		}

		buildingInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(building);
		TabContainer tabContainer = GetNode<TabContainer>("TabContainer");

		if (buildingInfo.buildingType.ToString() != "machine") { return; }

		if ((bool)buildingInfo.canChooseRecipe)
		{
			tabContainer.TabsVisible = true;
		}
		else
		{
			tabContainer.TabsVisible = false;
		}



		Label buildingName = GetNode<Label>("TabContainer/Building/Name");
		resourceProduction = GetNode<Label>("TabContainer/Building/Production");
			
		buildingName.Text = buildingInfo.name;
		dynamic recipe = recipes[buildingInfo.recipe.ToString()];
		resourceProduction.Text = "Recipe: none";

		if (buildingInfo.recipe.ToString() == "none")
		{
			tabContainer.CurrentTab = 0;
		}
		else
		{
			tabContainer.CurrentTab = 1;

			resourceProduction.Text = "Recipe: " + recipe.name.ToString();
			coordinates = new ((int)buildingInfo.coords[0], (int)buildingInfo.coords[1]);

			if (buildingInfo.type.ToString() == "drill")
			{
				InventorySlot slot = FindSlotByNameInGroup("SingleSlots", "DrillOutputSlot");
				slot.buildingCoordinates = coordinates;
				slot.itemType = buildingInfo.outputSlots[0].resource.ToString();
				slot.inventoryType = INVENTORYTYPE;
				slot.UpdateSlotTexture(slot.itemType);
				slot.Show();
			}
			else
			{
				Array<Node> inputSlots = GetTree().GetNodesInGroup("InputSlots");
				Array<Node> outputSlots = GetTree().GetNodesInGroup("OutputSlots");

				for (int i = 0; i < recipe.input.Count; i++)
				{
					InventorySlot slot = (InventorySlot)inputSlots[i];
					slot.buildingCoordinates = coordinates;
					slot.itemType = buildingInfo.inputSlots[i].resource.ToString();
					slot.inventoryType = INVENTORYTYPE;
					slot.UpdateSlotTexture(slot.itemType);
					slot.Show();
				}

				for (int i = 0; i < recipe.output.Count; i++)
				{
					InventorySlot slot = (InventorySlot)outputSlots[i];
					slot.buildingCoordinates = coordinates;
					slot.itemType = buildingInfo.outputSlots[i].resource.ToString();
					slot.inventoryType = INVENTORYTYPE;
					slot.UpdateSlotTexture(slot.itemType);
					slot.Show();
				}
			}
		}

		this.Show();

	}

	private void UpdateInventory(string info)
	{
		dynamic building = Newtonsoft.Json.JsonConvert.DeserializeObject(info);
		Vector2I coords = new Vector2I((int)building.coords[0], (int)building.coords[1]);
		
		if (coordinates != coords) { return; }
		
		productionProgress.Value = (double)building.productionProgress;

		if (building.type.ToString() == "drill")
		{	
			InventorySlot slot = GetNode<InventorySlot>("TabContainer/Building/Slots/DrillOutputSlot");
			UpdateSlot(slot, building.outputSlots[0].resource.ToString(), (int)building.outputSlots[0].amount);
		}
		else
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

		//GD.Print(coordinates.ToString());
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

	public InventorySlot FindSlotByNameInGroup(string groupName, string slotName)
	{
		Array<Node> slots = GetTree().GetNodesInGroup(groupName);

		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].Name ==slotName)
			{
				return (InventorySlot)slots[i];
			}
		}

		GD.PrintErr($"\"{slotName}\" is not in group");
		return new InventorySlot();
	} 
}