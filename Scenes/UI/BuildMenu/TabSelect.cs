using Godot;
using System;
using System.Security;

public partial class TabSelect : Button
{
	private Node parent;
	private TabContainer tabContainer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += ChangeTab;

		parent = GetParent();
		tabContainer = (TabContainer)GetTree().GetNodesInGroup("TabContainer")[0];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ChangeTab()
	{
		for (int i = 1; i < parent.GetChildCount() - 1; i++)
		{
			Button tab = (Button)parent.GetChild(i);
			tab.Disabled = false;
		}
		Disabled = true;
		tabContainer.CurrentTab = GetIndex() - 1;
	}
}
