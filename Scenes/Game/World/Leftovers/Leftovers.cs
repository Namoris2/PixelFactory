using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

public partial class Leftovers : Node2D
{
	private bool mouseOver;
	Rect2 rect;
	Inventories inventories;
	GameEvents gameEvents;
	public List<LeftoversSlot> items = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		rect = GetNode<TextureRect>("Icon").GetRect();
		inventories = GetNode<Inventories>("/root/main/UI/Inventories");
		gameEvents = GetNode<GameEvents>("/root/main/GameEvents");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 position = GetLocalMousePosition();
		mouseOver = rect.HasPoint(position);

		if (mouseOver)
		{
			GetNode<Label>("Label").Show();
			gameEvents.leftovers = this;
		}
		else
		{
			GetNode<Label>("Label").Hide();
			if (!inventories.Visible && gameEvents.leftovers == this)
			{
				gameEvents.leftovers = null;
			}
		}
	}    

	public void RemoveEmptySlots()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].itemType == "") { items.Remove(items[i]); i--; }
		}

		GD.Print(items.Count);

		if (items.Count == 0)
		{
			gameEvents.leftovers = null;
			QueueFree();
		}
	}
}
