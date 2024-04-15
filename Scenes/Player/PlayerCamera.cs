using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
	Vector2 minZoom = new (.4f, .4f);
	Vector2 maxZoom = new (2.4f, 2.4f);
	Vector2 zoomSpeed = new (.2f, .2f);


	[Signal]
	public delegate void CameraZoomChangedEventHandler(float zoom);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("ZoomIn") && this.Zoom[0] < maxZoom[0])
		{
			this.Zoom += zoomSpeed / 4;
		}

		if (Input.IsActionPressed("ZoomOut") && this.Zoom[0] > minZoom[0])
		{
			this.Zoom -= zoomSpeed / 4;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			// zoom in
			if (mouseEvent.ButtonIndex == MouseButton.WheelUp && this.Zoom[0] < maxZoom[0])
			{
				this.Zoom += zoomSpeed;
			}

			// zoom out
			if (mouseEvent.ButtonIndex == MouseButton.WheelDown && this.Zoom[0] > minZoom[0])
			{
				this.Zoom -= zoomSpeed;
			}
		}
	}
}

