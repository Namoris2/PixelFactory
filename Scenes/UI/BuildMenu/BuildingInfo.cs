using Godot;
using System;


public partial class BuildingInfo : Panel
{
	private Label name;
	private Label description;
	private Label needsHeader;
	private Label needs;
	dynamic buildings;
	dynamic items;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadFile load = new();
		buildings = load.LoadJson("buildings.json");
		items = load.LoadJson("items.json");

		name = GetNode<Label>("Name");
		description = GetNode<Label>("Description");
		needsHeader = GetNode<Label>("NeedsHeader");
		needs = GetNode<Label>("Needs");

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
			needs.Text += $"{building.cost[i].amount} x {items[building.cost[i].resource.ToString()].name}\n";
		}
	}

	public void HideBuildingInfo()
	{
		name.Text = "";
		description.Text = "";
		needsHeader.Hide();
		needs.Text = "";
	}
}
