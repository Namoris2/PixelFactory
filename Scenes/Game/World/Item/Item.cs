using Godot;
using System;
using System.Globalization;
using System.Reflection.Metadata;

public partial class Item : Node2D
{
	public Vector2 destination = new (0, 0);
	public Vector2I? parentBuilding;
	public bool onGround = false;
	public float speed = 0;
	public string itemType;

	private Label name;
	private AtlasTexture textureAtlas = new ();
	private TextureRect icon;

	private bool mouseHover = false;
	dynamic items;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{		
		LoadFile load = new();
		items = load.LoadJson("items.json");

		icon = GetNode<TextureRect>("Icon");
		name = GetNode<Label>("Name");

		textureAtlas.Atlas = GD.Load<Texture2D>("res://Gimp/Items/items.png");

		icon.MouseEntered +=  OnMouseEnter;
		icon.MouseExited += OnMouseExit;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (mouseHover && Input.IsActionPressed("Interact")) { PickUpItem(); }
		
		//GetNode<TextureRect>("ItemHolder").Visible = parentBuilding != null;
	}

    public override void _PhysicsProcess(double delta)
    {
        Position = Position.MoveToward(destination, speed * (float)delta);
    }

	// removes item from world and adds it to player's inventory 
	public bool PickUpItem()
    {
		PlayerInventory playerInventory = GetNode<PlayerInventory>("/root/main/UI/Inventories/InventoryGrid/PlayerInventory");
		InventorySlot[] inventorySlots = playerInventory.inventorySlots;

		bool canPutToInventory = false;
		if (playerInventory.PutToInventory(itemType, 1) == 0) { canPutToInventory = true; }

		if (canPutToInventory) 
		{ 
			QueueFree(); 
			if (!onGround)
			{
				World tileMap = GetNode<World>("/root/main/World/TileMap");

				dynamic building;
				if (parentBuilding == null)
				{
					string[] coordsArr = Name.ToString().Split('x');
					Vector2I coords = new (int.Parse(coordsArr[0]), int.Parse(coordsArr[1]));

					building = tileMap.GetBuildingInfo(coords);
				}
				else
				{
					building = tileMap.GetBuildingInfo((Vector2I)parentBuilding);
				}
				building.item = "";
				building.moveProgress = 0;
			}
		}

		return canPutToInventory;
    }

	public void UpdateItem(string itemType)
	{
		Vector2I atlasCoords = new Vector2I((int)items[itemType].atlasCoords[0], (int)items[itemType].atlasCoords[1]);
		textureAtlas.Region = new Rect2I(atlasCoords[0] * 16, atlasCoords[1] * 16, 16, 16);

		icon.Texture = textureAtlas;
		name.Text = items[itemType].name.ToString();
		this.itemType = itemType;
	}

	private void OnMouseEnter()
	{
		name.Show();
		mouseHover = true;
	}

	private void OnMouseExit()
	{
		name.Hide();
		mouseHover = false;
	}
}
