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
	Control items;

	float labelWidth;
	public override void _Ready()
	{
		button = GetNode<Button>("Button");
		icon = GetNode<TextureRect>("Button/HBoxContainer/Icon");
		nameLabel = GetNode<Label>("Button/HBoxContainer/Name");
		items = GetNode<Control>("Items");

		button.Pressed += ToggleCollapse;
		nameLabel.Text = name;
		labelWidth = nameLabel.Size.X;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void ToggleCollapse()
	{
		items.Visible = collapsed;
		collapsed = !collapsed;

		if (collapsed)
		{
			icon.Texture = icons[1];
		}
		else
		{
			icon.Texture = icons[0];
		}
	}
}
