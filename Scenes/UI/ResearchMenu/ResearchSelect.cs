using Godot;
using System;

public partial class ResearchSelect : Button
{
	Node parent;
	ResearchMenu researchMenu;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		parent = GetParent();
		researchMenu = (ResearchMenu)GetTree().GetFirstNodeInGroup("ResearchMenu");

		Pressed += Select;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void Select()
	{
		for (int i = 1; i < parent.GetChildCount() - 1; i++)
		{
			ResearchSelect researchSelect = (ResearchSelect)parent.GetChildren()[i];
			researchSelect.Disabled = false;
		}

		Disabled = true;
		researchMenu.ChangeTab(Name, Text);
	}
}
