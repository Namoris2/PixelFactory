using Godot;
using System;


public partial class BuildingInfo : Panel
{
	private Label name;
	private Label description;
	private Label needsHeader;
	private FlowContainer neededItems;
	private PlayerInventory playerInventory;
	dynamic buildings;
	dynamic items;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		buildings = LoadFile.LoadJson("buildings.json");
		items = LoadFile.LoadJson("items.json");

		name = GetNode<Label>("Name");
		description = GetNode<Label>("Description");
		needsHeader = GetNode<Label>("NeedsHeader");
		neededItems = GetNode<FlowContainer>("NeededItems");
		playerInventory = (PlayerInventory)GetTree().GetNodesInGroup("PlayerInventory")[0];

		HideBuildingInfo();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ShowBuildingInfo(string _building)
	{
		dynamic building = buildings[_building];

		name.Text = building.name.ToString();
		description.Text = building.description.ToString();
		needsHeader.Show();

		for (int i = 0; i < building.cost.Count; i++)
		{
			InventorySlot slot = (InventorySlot)neededItems.GetChild(i);
			string itemType = building.cost[i].resource.ToString();
			int totalItemAmount = playerInventory.GetItemAmount(itemType);

			slot.itemType = itemType;
			slot.resourceAmount.Text = $"{totalItemAmount}/{building.cost[i].amount.ToString()}";
			slot.UpdateSlotTexture(slot.itemType);
			slot.Show();
		}
	}

	public void HideBuildingInfo()
	{
		name.Text = "";
		description.Text = "";
		needsHeader.Hide();
		
		foreach (Node node in neededItems.GetChildren())
		{
			InventorySlot slot = (InventorySlot)node;
			slot.itemType = "";
			slot.resourceAmount.Text = "";
			slot.UpdateSlotTexture("");
			slot.Hide();
		}
	}
}
