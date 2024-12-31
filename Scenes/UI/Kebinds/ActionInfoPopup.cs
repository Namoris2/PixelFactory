using Godot;
using Godot.Collections;
using System;

public partial class ActionInfoPopup : HBoxContainer
{
	[Export] public string actionType;
	[Export] bool hideOnStart;
	[Export] string[] customActionText = new string[2];
	[Export] bool overrideWithCustomTextOnStart;

	public string actionText;
	Label label;
	ActionKey icon;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (hideOnStart) { Hide(); }
		label = GetNode<Label>("Label");
		icon = GetNode<ActionKey>("Icon");
		actionText = SettingsHandler.actions[actionType];

		SetDefaultActionText();
		if (overrideWithCustomTextOnStart) { SetCustomActionText();}

		SetActionIcon();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetCustomActionText(int index = 0)
	{
		if (customActionText[index] == "") { GD.PrintErr("No custom action text was set"); return; }
		label.Text = customActionText[index];
	}

	public void SetDefaultActionText()
	{
		label.Text = actionText;
	}

	public void SetActionIcon()
	{
		Array<InputEvent> inputs = InputMap.ActionGetEvents(actionType);
		if (inputs[0] is InputEventMouseButton)
		{
			icon.key = "Mouse" + (inputs[0] as InputEventMouseButton).ButtonIndex;
		}
		else
		{
			icon.key = inputs[0].AsText().TrimSuffix(" (Physical)");
		}
		icon.SetKeyIcon(icon.key);
	}
}
