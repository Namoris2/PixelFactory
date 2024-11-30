using Godot;
using System;

public partial class ActionKey : TextureRect
{
	[Export] public string key;

	AnimatedSprite2D sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		SetKeyIcon(key);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void SetKeyIcon(string keyString)
	{
		try
		{
			//string keyString = @event.AsText().Replace(" ", "");
			
			if (int.TryParse(keyString, out _))
			{
				keyString = "Num" + keyString;
			}

			sprite.Frame = (int)Enum.Parse(typeof(KeyboardKeys), keyString);	
		}
		catch (Exception)
		{
			sprite.Frame = (int)KeyboardKeys.Unknown;
		}

		GD.Print("Texture set");
		Texture = sprite.SpriteFrames.GetFrameTexture("default", sprite.Frame);
	}
}
