using Godot;
using System;

public partial class AnimatedBuildingPart : Node2D
{
	int rotation = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RotationDegrees = rotation * 90;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
