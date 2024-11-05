using Godot;
using System;

public partial class CollapsingMenu : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	[Export] Texture2D[] icons = new Texture2D[2];
	[Export] string name;
	bool collapsed = false;
	Button button;
	TextureRect icon;
	Label nameLabel;
	HFlowContainer items;
	public override void _Ready()
	{
		button = GetNode<Button>("Button");
		icon = GetNode<TextureRect>("Button/HBoxContainer/Icon");
		nameLabel = GetNode<Label>("Button/HBoxContainer/Name");
		items = GetNode<HFlowContainer>("Items");

		button.Pressed += ToggleCollapse;
		nameLabel.Text = name;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void ToggleCollapse()
	{
		collapsed = !collapsed;
		if (collapsed)
		{
			icon.Texture = icons[1];

			foreach (Control item in items.GetChildren())
			{
				item.Hide();
			}
		}
		else
		{
			icon.Texture = icons[0];

			foreach (Control item in items.GetChildren())
			{
				item.Show();
			}
		}
	}
}
