using Godot;
using System;
using System.Security;

public partial class TabSelect : Button
{
	private FlowContainer flowContainer;
	private Control buildMenu;
	private TabContainer tabContainer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += ChangeTab;

		flowContainer = (FlowContainer)GetParent();
		buildMenu = (Control)GetParent().GetParent();
		tabContainer = buildMenu.GetNode<TabContainer>("TabContainer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ChangeTab()
	{
		for (int i = 0; i < flowContainer.GetChildCount(); i++)
		{
			Button tab = (Button)flowContainer.GetChild(i);
			tab.Disabled = false;
			Disabled = true;
		}
		tabContainer.CurrentTab = GetIndex();
	}
}
