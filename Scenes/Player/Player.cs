using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : Godot.CharacterBody2D
{
	public int Speed { get; set; } = 400;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("Sprint"))
		{
			Speed = 700;
		}
		else 
		{
			Speed = 400;
		}
	}
	public void GetInput()
	{
		Vector2 inputDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
		Velocity = inputDirection * Speed; 
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput();
		MoveAndSlide();
		UpdatePlayerTexture();
	}

	private void UpdatePlayerTexture()
	{
		Sprite2D playerSprite = GetNode<Sprite2D>("PlayerIcon");
		string path = "res://Gimp/Player/";

		if (Input.IsActionPressed("MoveLeft"))
		{
			playerSprite.Texture = GD.Load<Texture2D>(path + "playerPlaceholderLeft.png");
		}
		else if (Input.IsActionPressed("MoveRight"))
		{
			playerSprite.Texture = GD.Load<Texture2D>(path + "playerPlaceholderRight.png");
		}
		else if (Input.IsActionPressed("MoveUp"))
		{
			playerSprite.Texture = GD.Load<Texture2D>(path + "playerPlaceholderUp.png");
		}
		else
		{
			playerSprite.Texture = GD.Load<Texture2D>(path + "playerPlaceholder.png");
		}
	}
}
