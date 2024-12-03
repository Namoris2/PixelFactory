using Godot;
using System;

public partial class InputMapTest : Node
{
	public override void _Ready()
	{
        GD.Print(InputMap.ActionGetEvents("Sprint"));
	}
}
