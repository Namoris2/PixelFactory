using Godot;
using System;
using System.Reflection.Metadata;

public partial class Item : Control
{
	public Vector2 destination = new (0, 0);
	public bool onGround = false;
	public float speed = 0;

	private Label name;
	private AtlasTexture textureAtlas = new ();
	private TextureRect icon;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		icon = GetNode<TextureRect>("Icon");
		name = GetNode<Label>("Name");

		textureAtlas.Atlas = GD.Load<Texture2D>("res://Gimp/items/items.png");

		MouseEntered += name.Show;
		MouseExited += name.Hide;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _PhysicsProcess(double delta)
    {
        Position = Position.MoveToward(destination, speed * (float)delta);

    }

    

	public void UpdateItem(string itemType)
	{
		LoadFile load = new();
		dynamic items = load.LoadJson("items.json");

		Vector2I atlasCoords = new Vector2I((int)items[itemType].atlasCoords[0], (int)items[itemType].atlasCoords[1]);
		textureAtlas.Region = new Rect2I(atlasCoords[0] * 16, atlasCoords[1] * 16, 16, 16);

		icon.Texture = textureAtlas;
		name.Text = items[itemType].name.ToString();
	}
}
