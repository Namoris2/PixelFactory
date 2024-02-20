using Godot;
using System;

public partial class ItemName : Label
{
	Vector2 position;
	Vector2 slotPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TextureRect item = GetNode<TextureRect>("../ItemTexture");
		item.MouseEntered += ShowItemName;
		item.MouseExited += HideItemName;

		// GlobalPosition = GlobalPosition - new Vector2(20, 20);
		// slotPosition = GetParent<Button>().GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/*if (slotPosition != GetParent<Button>().GlobalPosition)
		{
			//GD.Print("Window Size Changed");
			slotPosition = GetParent<Button>().GlobalPosition;
			position = GlobalPosition - new Vector2(20, 20);
		}*/
		
		if (Visible)
		{
			GlobalPosition = GetGlobalMousePosition() + new Vector2(20, 20);
		}

		if(Input.IsActionJustPressed("Interact"))
		{
			HideItemName();
		}
	}

	private void ShowItemName()
	{
		InventorySlot slot = this.GetParent<InventorySlot>();
		
		if (slot.itemType == "") { return; }


		LoadFile load = new();
		dynamic items = load.LoadJson("items.json");
		this.Text = items[slot.itemType].name.ToString();
		this.Show();
	}
	private void HideItemName()
	{
		this.Hide();
	}
}
