using Godot;
using System;

public partial class HoldingItem : TextureRect
{
	[Export]
	public string itemName;
	[Export]
	public string itemAmount;

	public bool ISHOLDINGITEM = false;
	
	Vector2 position;
	private dynamic items;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		items = LoadFile.LoadJson("items.json");

		position = GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (ISHOLDINGITEM)
		{
			this.Position = GetGlobalMousePosition() - this.Size / 2;
		}
	}

	public void ShowHoldingItem(string type, string amount)
	{
		itemName = type;
		itemAmount = amount;
		ISHOLDINGITEM = true;

		GetNode<Label>("ResourceAmount").Text = itemAmount;
		Texture = main.GetTexture("items.json", type);

		Show();
	}

	public void HideHoldingItem()
	{
		itemName = "";
		itemAmount = "";
		ISHOLDINGITEM = false;

		GetNode<Label>("ResourceAmount").Text = "";

		Hide();
	}
}
