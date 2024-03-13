using Godot;
using System;
using System.Dynamic;

public partial class InventorySlot : Button
{
	[Signal]
	public delegate void ShowHoldingItemEventHandler(string name, string amount);	
	[Signal]
	public delegate void HideHoldingItemEventHandler();

	[Export]
	public bool UserImport = true;	
	[Export]
	public bool UserExport = true;
	[Export]
	public string itemType = "";

	private string slotType;
	private Item inventoryItem;
	private AtlasTexture textureAtlas = new ();
	private TextureRect itemTexture;
	public Label resourceAmount;
	private Label resourceName;
	public int inventorySlotIndex;
	public string inventoryType;
	public Vector2I buildingCoordinates;
	private World tileMap;

	public dynamic items;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadFile load = new();
		items = load.LoadJson("items.json");

		itemTexture = GetNode<TextureRect>("ItemTexture");
		resourceAmount = GetNode<Label>("ResourceAmount");
		resourceName = GetNode<Label>("ItemName");
		
		inventorySlotIndex = this.GetIndex();

		textureAtlas.Atlas = GD.Load<Texture2D>("res://Gimp/Items/items.png");
		tileMap = GetNode<World>("/root/main/World/TileMap");

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

		if (IsInGroup("SingleSlots"))
		{
			inventorySlotIndex = 0;
		}

		this.Pressed += PressedSlot;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void UpdateSlotTexture(string itemType)
	{
		if (resourceAmount.Text != "" && itemType != "")
		{
			Vector2I atlasCoords = new Vector2I((int)items[itemType].atlasCoords[0], (int)items[itemType].atlasCoords[1]);

			textureAtlas.Region = new Rect2I(atlasCoords[0] * 16, atlasCoords[1] * 16, 16, 16);

			itemTexture.Texture = textureAtlas;
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

		// taking item from slot
		if (!holdingItem.ISHOLDINGITEM && UserExport)
		{
			if (resourceAmount.Text == "") { return; };

			EmitSignal(SignalName.ShowHoldingItem, itemType, resourceAmount.Text);
			if (inventoryType == "machine")
			{
				World tileMap = GetNode<World>("/root/main/World/TileMap");
				LoadFile load = new ();
				
				tileMap.RemoveItemFromSlot(buildingCoordinates, slotType, inventorySlotIndex);
				
				dynamic building = tileMap.GetBuildingInfo(buildingCoordinates);
				dynamic recipe = load.LoadJson("recipes.json")[building.recipe.ToString()];

				itemType = recipe[slotType.ToLower()][inventorySlotIndex].name;
				building[$"{slotType.ToLower()}Slots"][inventorySlotIndex].resource = itemType;
			}
			
			if (inventoryType != "machine")
			{
				itemType = "";
			}

			resourceAmount.Text = "";
			itemTexture.Texture = null;

			if (inventoryType == "storage")
			{
				tileMap.RemoveItemFromSlot(buildingCoordinates, slotType, inventorySlotIndex);
			}
		}

		// putting item to slot
		else if (holdingItem.ISHOLDINGITEM && UserImport)
		{
			if (inventoryType == "machine" && holdingItem.itemName == itemType)
			{
				World tileMap = GetNode<World>("/root/main/World/TileMap");

				tileMap.PutItemToSlot(buildingCoordinates, int.Parse(holdingItem.itemAmount), itemType, slotType, inventorySlotIndex);
			}

			// putting item to slot (if slot is empty)
			if (itemType == "")
			{
				itemType = holdingItem.itemName;
				resourceName.Text = itemType;
				resourceAmount.Text = holdingItem.itemAmount;
				UpdateSlotTexture(itemType);

				EmitSignal(SignalName.HideHoldingItem);
			}

			// putting item to slot (if slot is not empty)
			else if (itemType == holdingItem.itemName)
			{
				int amount = 0;
				if (resourceAmount.Text != "")
				{
					amount = int.Parse(resourceAmount.Text);
				}
				// putting item to slot (if sum of amount of item in slot and amount of holding item is less or equal to its 'maxStackSize')
				if ((amount + int.Parse(holdingItem.itemAmount)) <= (int)items[itemType].maxStackSize)
				{
					resourceAmount.Text = (amount + int.Parse(holdingItem.itemAmount)).ToString();
					EmitSignal(SignalName.HideHoldingItem);
				}

				// putting item to slot (if sum of amount of item in slot and amount of holding item is more or equal to its 'maxStackSize')
				// amount of item in slot is set to its 'maxStackSize' and amount of holding item is set to new value (current amount - amount of items put to slot)
				else
				{
					holdingItem.itemAmount = (int.Parse(holdingItem.itemAmount) - ((int)items[itemType].maxStackSize - amount)).ToString();
					resourceAmount.Text = items[itemType].maxStackSize.ToString();
					GetNode<Label>(holdingItem.GetPath() + "/ResourceAmount").Text = holdingItem.itemAmount;
				}
				UpdateSlotTexture(itemType);
			}

			// switching item in slot and holding item (if item in slot is different from holding item)
			else
			{
				string helperItemType;
				string helperResourceAmount;
				Texture2D helperTexture;

				helperItemType = holdingItem.itemName;
				helperResourceAmount = holdingItem.itemAmount;
				helperTexture = holdingItem.Texture;

				holdingItem.itemName = itemType;
				holdingItem.itemAmount = resourceAmount.Text;
				GetNode<Label>(holdingItem.GetPath() + "/ResourceAmount").Text = holdingItem.itemAmount; 
				EmitSignal(SignalName.ShowHoldingItem, itemType, resourceAmount.Text);

				itemType = helperItemType;
				resourceAmount.Text = helperResourceAmount;
				itemTexture.Texture = helperTexture;
				//UpdateSlotTexture(itemType);
			}

			if (inventoryType == "storage")
			{
				int amount = 0;

				if (resourceAmount.Text != "")
				{
					amount = int.Parse(resourceAmount.Text);
				}

				tileMap.PutItemToSlot(buildingCoordinates, amount, itemType, slotType, inventorySlotIndex);
			}
		}

		this.ShowHoldingItem -= holdingItem.ShowHoldingItem;
		this.HideHoldingItem -= holdingItem.HideHoldingItem;
	}
}
