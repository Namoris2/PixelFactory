using Godot;
using System;

public partial class TabSelection : Button
{
	[Export] int index = -1;
	BuildingInventory buildingInventory;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		buildingInventory = (BuildingInventory)GetTree().GetFirstNodeInGroup("BuildingInventory");
		Pressed += ChangeTab;

		if(index == -1)
		{
			index = GetIndex();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ChangeTab()
	{
		buildingInventory.tabContainer.CurrentTab = index;
	}
}
