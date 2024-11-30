using Godot;
using System;
using System.Collections.Generic;

public partial class KeyBindsMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	readonly Dictionary<string, string> actions = new()
	{
		{ "MoveUp", "Move Up" },
		{ "MoveDown", "Move Down" },
		{ "MoveLeft", "Move Left" },
		{ "MoveRight", "Move Right" },
		{ "Sprint", "Sprint" },
		{ "Interact", "Interact" },
		{ "Rotate", "Rotate Building" },
		{ "ToggleBuildMode", "Open BuildMenu" },
		{ "ToggleDismantleMode", "Dismantle Mode" },
		{ "ToggleInventory", "Open Inventory" },
	};

	Node buttonsContainer;
	Button resetBinds;
	
	public override void _Ready()
	{
		buttonsContainer = GetNode<Node>("VBoxContainer");
		resetBinds = GetNode<Button>("ResetBinds");
		resetBinds.Pressed += ResetBinds;
		
		foreach (KeyValuePair<string,string> action in actions)
		{
			KeyBindEdit keyBindEdit = (KeyBindEdit)GD.Load<PackedScene>("res://Scenes/UI/Settings/KeyBindEdit.tscn").Instantiate();
			Label actionLabel = keyBindEdit.GetNode<Label>("HBoxContainer/Action");
			ActionKey icon = keyBindEdit.GetNode<ActionKey>("HBoxContainer/ActionKey");
			Label keyLabel = keyBindEdit.GetNode<Label>("HBoxContainer/Key");

			keyBindEdit.actionType = action.Key;
			actionLabel.Text = action.Value;

			Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action.Key);
			icon.key = events[0].AsText().TrimSuffix(" (Physical)");

			buttonsContainer.AddChild(keyBindEdit);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void ResetBinds()
	{
		foreach (KeyBindEdit keyBindEdit in buttonsContainer.GetChildren())
		{
			keyBindEdit.SetBind(SettingsHandler.defaultBinds[keyBindEdit.actionType]);
		}
	}
}
