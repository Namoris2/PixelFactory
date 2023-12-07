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
		LoadFile load = GetNode<LoadFile>("/root/main/LoadFile");
		items = load.LoadJson("items.json");

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

	public void ShowHoldingItem(string type, string amount, Texture2D texture)
	{
		itemName = type;
		itemAmount = amount;
		ISHOLDINGITEM = true;

		GetNode<Label>("ResourceAmount").Text = itemAmount;
		this.Texture = texture;

		this.Show();
	}

	public void HideHoldingItem()
	{
		itemName = "";
		itemAmount = "";
		ISHOLDINGITEM = false;

		GetNode<Label>("ResourceAmount").Text = "";

		this.Hide();
	}
}
