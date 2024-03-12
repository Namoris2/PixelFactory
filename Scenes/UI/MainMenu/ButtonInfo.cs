using Godot;
using Godot.Collections;
using System;

public partial class ButtonInfo : Label
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Array<Node> buttons = GetTree().GetNodesInGroup("MenuButton");

		for (int i = 0; i < buttons.Count; i++)
		{
			ChangeMenu button = (ChangeMenu)buttons[i];
			button.ShowInfo += Show;
			button.HideInfo += Hide;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Visible)
		{
			GlobalPosition = GetGlobalMousePosition() + new Vector2(20, 20);
		}
	}
}
