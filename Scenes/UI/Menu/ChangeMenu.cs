using Godot;
using System;

public partial class ChangeMenu : Button
{
	[Export] 
	int HomeIndex = 0;
	[Export] 
	int IndexOffset = 0;
	[Export] 
	bool GoHome = false;

	private TabContainer tabContainer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += ChangeTab;

		tabContainer = (TabContainer)GetTree().GetNodesInGroup("TabContainer")[0];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ChangeTab()
	{
		if (GoHome)
		{
			tabContainer.CurrentTab = HomeIndex;
		}
		else
		{
			tabContainer.CurrentTab = GetIndex() + IndexOffset;
		}
		GD.Print(tabContainer.CurrentTab, tabContainer.GetChild(tabContainer.CurrentTab).Name);
	}
}
