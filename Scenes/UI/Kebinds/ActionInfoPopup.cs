using Godot;
using Godot.Collections;
using System;

public partial class ActionInfoPopup : HBoxContainer
{
	[Export] string actionType;
	[Export] string customActionText;
	Label label;
	ActionKey icon;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		label = GetNode<Label>("Label");
		icon = GetNode<ActionKey>("Icon");

		if (customActionText == "")
		{
			label.Text = SettingsHandler.actions[actionType];
		}
		else
		{
			label.Text = customActionText;
		}

		Array<InputEvent> inputs = InputMap.ActionGetEvents(actionType);
		icon.key = inputs[0].AsText().TrimSuffix(" (Physical)");
		icon.SetKeyIcon(icon.key);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
