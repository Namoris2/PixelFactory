using Godot;
using System;

public partial class BuildingPart : Node2D
{
	public int rotation = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Node2D>("Part").RotationDegrees = rotation * 90;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
