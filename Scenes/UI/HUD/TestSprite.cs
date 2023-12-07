using Godot;
using System;

public partial class TestSprite : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AtlasTexture texture = new AtlasTexture();
		texture.Atlas = GD.Load<Texture2D>("res://Gimp/items/items.png");
		texture.Region = new Rect2I(32, 0, 16, 16);

		this.Texture = texture;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
