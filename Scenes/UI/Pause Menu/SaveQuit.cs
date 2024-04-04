using Godot;
using System;

public partial class SaveQuit : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += QuitGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void QuitGame()
	{
		GetNode<SaveGame>("../SaveGame").Save();
		GetNode<Node2D>("/root/main").QueueFree();
		MainMenu mainMenu = (MainMenu)GD.Load<PackedScene>("res://Scenes/UI/MainMenu/MainMenu.tscn").Instantiate();
		GetTree().Root.AddChild(mainMenu);
		GetTree().Paused = false;
	}
}
