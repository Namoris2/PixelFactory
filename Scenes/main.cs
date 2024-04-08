using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class main : Node
{
	public string savePath;
	public int seed;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{		
	}

	static public Node FindNodeByNameInGroup(Array<Node> group, string nodeName)
	{
		for (int i = 0; i < group.Count; i++)
		{
			if (group[i].Name == nodeName)
			{
				return group[i];
			}
		}

		GD.PrintErr($"\"{nodeName}\" is not in group");
		return null;
	}
}
