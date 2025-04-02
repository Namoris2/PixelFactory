using Godot;
using Godot.Collections;
using System;

public partial class KeyBindEdit : Button
{
	// Called when the node enters the scene tree for the first time.
	SettingsHandler settingsHandler;
	public InputEvent defaultKeyBind;
	Label keyLabel;
	ActionKey icon;
	bool editingAction = false;
	[Export] public string actionType;
	public override void _Ready()
	{
		settingsHandler = GetNode<SettingsHandler>("/root/SettingsHandler");
		keyLabel = GetNode<Label>("HBoxContainer/Key");
		icon = GetNode<ActionKey>("HBoxContainer/ActionKey");
		Pressed += ToggleEditing;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (!editingAction || !@event.IsPressed()) { return; }

		if (Input.IsKeyPressed(Key.Escape))
		{ 
			keyLabel.Text = InputMap.ActionGetEvents(actionType)[0].AsText().TrimSuffix(" (Physical)");
			editingAction = false;
			Disabled = false;
			return; 
		}
		
		SetBind(@event);
	}

	void ToggleEditing()
	{
		editingAction = true;
		Disabled = true;
		keyLabel.Show();
		icon.Hide();
	}

	public void SetBind(InputEvent @event)
	{
		Array<InputEvent> inputs = InputMap.ActionGetEvents(actionType);
		inputs[0] = @event;
		InputMap.ActionEraseEvents(actionType);

		for (int i = 0; i < inputs.Count; i++)
		{
			InputMap.ActionAddEvent(actionType, inputs[i]);
		}

		editingAction = false;
		Disabled = false;

		string bind = inputs[0].AsText().TrimSuffix(" (Physical)");
		if (@event is InputEventMouse)
		{
			InputEventMouseButton button = @event as InputEventMouseButton;
			bind = "Mouse" + button.ButtonIndex;
		}

		icon.key = bind;
		icon.SetKeyIcon(bind);

		settingsHandler.SaveConfigFile("KeyboardMouseBinds", actionType, bind);

		keyLabel.Hide();
		icon.Show();

		foreach (ActionInfoPopup popup in GetTree().GetNodesInGroup("Popup"))
		{
			if (popup.actionType == actionType) { popup.SetActionIcon(); }
		}
	}
}
