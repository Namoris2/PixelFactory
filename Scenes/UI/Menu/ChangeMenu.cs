using Godot;
using System;

public partial class ChangeMenu : Button
{
	[Export] int HomeIndex = 0;
	[Export] int IndexOffset = 0;
	[Export] bool GoHome = false;
	[Export] string GroupName;

	[Signal] public delegate void ShowInfoEventHandler();

	[Signal] public delegate void HideInfoEventHandler();


	private TabContainer tabContainer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += ChangeTab;
		
		tabContainer = (TabContainer)GetTree().GetNodesInGroup(GroupName)[0];

		MouseEntered += ShowButtonInfo;
		MouseExited += HideButtonInfo;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ChangeTab()
	{
		//GD.Print("Clicked");
		if (GoHome)
		{
			tabContainer.CurrentTab = HomeIndex;
		}
		else
		{
			tabContainer.CurrentTab = GetIndex() + IndexOffset;
		}

		if (GetParent().Name == "NewGame")
		{
			GetNode<LineEdit>("../SaveNameInput").Text = "";
			GetNode<LineEdit>("../SeedInput").Text = "";
			GetNode<Label>("../ErrorMessage").Text = "";
		}
		//GD.Print(tabContainer.CurrentTab, tabContainer.GetChild(tabContainer.CurrentTab).Name);
	}

	private void ShowButtonInfo()
	{
		if (Disabled)
		{
			EmitSignal(SignalName.ShowInfo);
		}
	}

	private void HideButtonInfo()
	{
		EmitSignal(SignalName.HideInfo);
	}
}

