using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class main : Node2D
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

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionPressed("CursorLeft") || Input.IsActionPressed("CursorRight") || Input.IsActionPressed("CursorUp") || Input.IsActionPressed("CursorDown"))
		{
			Vector2 inputDirection = Input.GetVector("CursorLeft", "CursorRight", "CursorUp", "CursorDown");
			//GD.Print(GetViewport().GetMousePosition(), inputDirection * 3);
			Input.WarpMouse(GetViewport().GetMousePosition() + inputDirection * 10);
		}
	}

	public static void QueueFreeAllChildren(Node node)
	{
		Array<Node> children = node.GetChildren();

		if (children.Count == 0)
		{
			node.QueueFree();
			return;
		}

		foreach (Node child in children)
		{
			QueueFreeAllChildren(child);
		}

		node.QueueFree();
	}
}
