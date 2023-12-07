using Godot;
using System;
using System.Dynamic;

public partial class InventorySlot : Button
{
	[Signal]
	public delegate void SlotClickedEventHandler(Vector2I coords, string type, int slotIndex);
	[Signal]
	public delegate void ShowHoldingItemEventHandler(string name, string amount, Texture2D texture);	
	[Signal]
	public delegate void HideHoldingItemEventHandler();

	[Export]
	public bool UserInput = true;	
	[Export]
	public bool UserExport = true;
	[Export]
	public string itemType = "";

	private string slotType;
	private Item inventoryItem;
	private AtlasTexture textureAtlas = new ();
	private TextureRect itemTexture;
	private Label resourceAmount;
	private Label resourceName;
	public int inventorySlotIndex;
	public string inventoryType;
	public Vector2 buildingCoordinates;

	private dynamic items;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadFile load = GetNode<LoadFile>("/root/main/LoadFile");
		items = load.LoadJson("items.json");

		itemTexture = GetNode<TextureRect>("ItemTexture");
		resourceAmount = GetNode<Label>("ResourceAmount");
		resourceName = GetNode<Label>("ItemName");
		
		inventorySlotIndex = this.GetIndex();

		textureAtlas.Atlas = GD.Load<Texture2D>("res://Gimp/items/items.png");

		string name = this.Name;
		if(name.Contains("Input"))
		{
			slotType = "Input";
		}
		else if(name.Contains("Output"))
		{
			slotType = "Output";
		}
		else
		{
			slotType = "Slot";
		}

		this.Pressed += PressedSlot;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void UpdateSlotTexture(string itemType)
	{
		if (resourceAmount.Text != "")
		{
			Vector2I atlasCoords = new Vector2I((int)items[itemType].atlasCoords[0], (int)items[itemType].atlasCoords[1]);

			textureAtlas.Atlas = GD.Load<Texture2D>("res://Gimp/items/items.png");
			textureAtlas.Region = new Rect2I(atlasCoords[0] * 16, atlasCoords[1] * 16, 16, 16);

			itemTexture.Texture = textureAtlas;
			GD.Print(itemType);
		}
		else
		{
			itemTexture.Texture = null;
		}
	}

	private void PressedSlot() 
	{
		HoldingItem holdingItem = GetNode<HoldingItem>("/root/main/UI/Inventories/HoldingItem");

		this.ShowHoldingItem += holdingItem.ShowHoldingItem;
		this.HideHoldingItem += holdingItem.HideHoldingItem;

		if (!holdingItem.ISHOLDINGITEM)
		{
			if (inventoryType == "machine" && UserExport)
			{
				TileMap tileMap = GetNode<TileMap>("/root/main/World/TileMap");
				this.SlotClicked += tileMap.RemoveItemFromSlot;

				EmitSignal(SignalName.SlotClicked, buildingCoordinates, slotType, inventorySlotIndex);
			}
			EmitSignal(SignalName.ShowHoldingItem, itemType, resourceAmount.Text, itemTexture.Texture);
			
			itemType = "";
			resourceAmount.Text = "";
			itemTexture.Texture = null;

			this.ShowHoldingItem -= holdingItem.ShowHoldingItem;
			return;
		}

		if (holdingItem.ISHOLDINGITEM)
		{
			itemType = holdingItem.itemName;
			resourceName.Text = itemType;
			resourceAmount.Text = holdingItem.itemAmount;
			UpdateSlotTexture(itemType);

			EmitSignal(SignalName.HideHoldingItem);
			this.HideHoldingItem -= holdingItem.HideHoldingItem;
			return;
		}

	}
}
