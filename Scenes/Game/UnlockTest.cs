using Godot;
using System;

public partial class UnlockTest : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += Unlock;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void Unlock()
	{
		Research research = (Research)GetTree().GetFirstNodeInGroup("Research");
		research.UnlockResearch("PartAssembly");
	}
}
