using Godot;
using System;

public partial class EnoughResourcesLabel : Label
{
    public override void _Process(double delta)
    {
		if (Visible)
		{
			GlobalPosition = GetGlobalMousePosition() + new Vector2(20, 20);
		}
    }

}
