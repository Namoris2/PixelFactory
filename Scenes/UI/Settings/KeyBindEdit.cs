using Godot;
using Godot.Collections;
using System;

public partial class KeyBindEdit : Button
{
	// Called when the node enters the scene tree for the first time.
	Label keyLabel;
	bool editingAction = false;
	[Export] public string actionType;
	public override void _Ready()
	{
		keyLabel = GetNode<Label>("HBoxContainer/Key");
		Pressed += UpdateKey;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (!editingAction || !@event.IsPressed() /*@event is not InputEventKey*/) { return; }

		if (Input.IsKeyPressed(Key.Escape))
		{ 
			keyLabel.Text = InputMap.ActionGetEvents(actionType)[0].AsText().TrimSuffix(" (Physical)");
			editingAction = false;
			Disabled = false;
			return; 
		}
		
		Array<InputEvent> inputs = InputMap.ActionGetEvents(actionType);
		inputs[0] = @event;
		InputMap.ActionEraseEvents(actionType);

		for (int i = 0; i < inputs.Count; i++)
		{
			InputMap.ActionAddEvent(actionType, inputs[i]);
		}

		keyLabel.Text = inputs[0].AsText();
		editingAction = false;
		Disabled = false;

		/*GD.Print(inputs);
		GD.Print(InputMap.ActionGetEvents(actionType), "\n");*/
	}

	void UpdateKey()
	{
		editingAction = true;
		Disabled = true;
		keyLabel.Text = "Press Key to Bind...";
	}
}
