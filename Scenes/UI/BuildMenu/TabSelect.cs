using Godot;
using System;
using System.Security;

public partial class TabSelect : Button
{
	private Node partent;
	private TabContainer tabContainer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += ChangeTab;

		partent = GetParent();
		tabContainer = (TabContainer)GetTree().GetNodesInGroup("TabContainer")[0];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ChangeTab()
	{
		for (int i = 0; i < partent.GetChildCount(); i++)
		{
			Button tab = (Button)partent.GetChild(i);
			tab.Disabled = false;
			Disabled = true;
		}
		tabContainer.CurrentTab = GetIndex();
	}
}
