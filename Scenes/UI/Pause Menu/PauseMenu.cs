using Godot;
using System;

public partial class PauseMenu : Control
{
	[Export]
	bool pauseOnStart = true;
	public bool CanPause = true;
	Label worldInfo;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		worldInfo = GetNode<Label>("../WorldInfo");
		if (pauseOnStart) {
			GetTree().Paused = true;
			Show();
		}
		else
		{
			Hide();
		}

		Button close = GetNode<Button>("Close");
		close.Pressed += UnpauseGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PauseGame()
	{
		Visible = true;
		GetTree().Paused = !GetTree().Paused;
		worldInfo.Hide();
	}

	public void UnpauseGame()
	{
		Visible = false;
		GetTree().Paused = false;
		worldInfo.Show();
	}
}
