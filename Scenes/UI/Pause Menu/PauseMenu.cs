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

		GameEvents.closePopup.SetDefaultActionText();
		GameEvents.closePopup.Show();
		GameEvents.pickUpItemPopup.Hide();
		GameEvents.harvestResourcePopup.Hide();
		GameEvents.toggleInventoryPopup.Hide();
		GameEvents.toggleBuildingInventoryPopup.Hide();
		GameEvents.toggleBuildMenuPopup.Hide();
		GameEvents.toggleDismantleModePopup.Hide();
		GameEvents.toggleResearchMenuPopup.Hide();
		GameEvents.collectedItemsContainer.Hide();
		GameEvents.tutorialContainer.Hide();
	}

	public void UnpauseGame()
	{
		Visible = false;
		GetTree().Paused = false;
		worldInfo.Show();

		GameEvents.closePopup.Hide();
		GameEvents.closePopup.SetCustomActionText();
		GameEvents.toggleInventoryPopup.Show();
		GameEvents.toggleBuildMenuPopup.Show();
		GameEvents.toggleDismantleModePopup.Show();
		GameEvents.toggleResearchMenuPopup.Show();
		GameEvents.collectedItemsContainer.Show();
		GameEvents.tutorialContainer.Show();
	}
}
