using Godot;
using System;

public partial class ItemName : Label
{
	Vector2 position;
	Vector2 slotPosition;
	TextureRect item;
	HoldingItem holdingItem;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		item = GetNode<TextureRect>("../ItemTexture");
		holdingItem = GetNode<HoldingItem>("/root/main/UI/Inventories/HoldingItem");

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
		InventorySlot slot = GetParent<InventorySlot>();
		if (slot.UserExport && !holdingItem.ISHOLDINGITEM && slot.resourceAmount.Text != "") 
		{
			GameEvents.splitStackPopup.Show(); 
			GameEvents.splitStackPopup.SetDefaultActionText();
		}
		else if (slot.UserImport && holdingItem.ISHOLDINGITEM && (slot.itemType == holdingItem.itemName || slot.itemType == ""))
		{
			GameEvents.splitStackPopup.Show(); 
			GameEvents.splitStackPopup.SetCustomActionText();
		}
		
		if (slot.itemType == "") { return; }

		LoadFile load = new();
		dynamic items = LoadFile.LoadJson("items.json");
		Text = items[slot.itemType].name.ToString();
		Size = new (0, 0);
		Show();
	}
	
	private void HideItemName()
	{
		GameEvents.splitStackPopup.Hide();
		Hide();
	}
}
