using Godot;
using System;


public partial class TestLabel : Label
{
	bool hidden = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public void HideToggle()
	{
		hidden = !hidden;

		if (hidden) { this.Hide(); }
		else { this.Show(); }
	}
}
