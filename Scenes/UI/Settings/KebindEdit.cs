using Godot;
using Godot.Collections;
using System;

public partial class KebindEdit : Button
{
	// Called when the node enters the scene tree for the first time.
	bool editingAction = false;
	[Export] string actionType;
	public override void _Ready()
	{
		Pressed += UpdateKey;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void UpdateKey()
	{
		editingAction = true;
		Text = "Press Key";
	}

	public override void _Input(InputEvent @event)
	{
		if (!editingAction || @event is not InputEventKey) { return; }

		if (Input.IsKeyPressed(Key.Escape))
		{ 
			Text = "Edit Key";
			editingAction = false;
			return; 
		}
		
		Array<InputEvent> inputs = InputMap.ActionGetEvents(actionType);
		inputs[0] = @event;
		InputMap.ActionEraseEvents(actionType);

		for (int i = 0; i < inputs.Count; i++)
		{
			InputMap.ActionAddEvent(actionType, inputs[i]);
		}

		Text = "Edit Key";
		editingAction = false;

		/*GD.Print(inputs);
		GD.Print(InputMap.ActionGetEvents(actionType), "\n");*/
	}
}
