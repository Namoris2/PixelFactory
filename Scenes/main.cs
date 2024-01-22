using Godot;
using Godot.Collections;
using System;

public partial class main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{		
	}

	public Node FindNodeByNameInGroup(string groupName, string nodeName)
	{
		Array<Node> nodes = GetTree().GetNodesInGroup(groupName);

		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].Name == nodeName)
			{
				return (InventorySlot)nodes[i];
			}
		}

		GD.PrintErr($"\"{nodeName}\" is not in group");
		return new Node();
	}
}
