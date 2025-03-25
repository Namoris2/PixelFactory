using Godot;
using System;
using System.Collections.Generic;
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
	private Sprite2D icon;

	public bool mouseHover = false;
	dynamic items;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{		
		items = LoadFile.LoadJson("items.json");

		icon = GetNode<Sprite2D>("Icon");
		name = GetNode<Label>("Name");

		textureAtlas.Atlas = GD.Load<Texture2D>("res://Gimp/Items/items.png");

		Area2D area2D = GetNode<Area2D>("Area2D");

		area2D.MouseEntered +=  OnMouseEnter;
		area2D.MouseExited += OnMouseExit;
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
		List<InventorySlot> inventorySlots = playerInventory.inventorySlots;

		bool canPutToInventory = false;
		if (playerInventory.PutToInventory(itemType, 1) == 0) { canPutToInventory = true; }

		if (canPutToInventory)
		{ 
			QueueFree(); 
			GameEvents.pickUpItemPopup.Hide();

			CollectedItemsContainer collectedItemsContainer = GetNode<CollectedItemsContainer>("/root/main/UI/CollectedItemsContainer");
			collectedItemsContainer.ShowCollectedItem(itemType, 1);

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
		icon.Texture = main.GetTexture("items.json", itemType);
		name.Text = items[itemType].name.ToString();
		this.itemType = itemType;
	}

	private void OnMouseEnter()
	{
		name.Show();
		GameEvents.pickUpItemPopup.Show();
		mouseHover = true;
	}

	private void OnMouseExit()
	{
		name.Hide();
		GameEvents.pickUpItemPopup.Hide();
		mouseHover = false;
	}
}
