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
	}

	public override void _Input(InputEvent @event)
	{
		Sprite2D playerSprite = GetNode<Sprite2D>("");

		if (@event.IsActionPressed("MoveLeft"))
		{

		}
	}
}
