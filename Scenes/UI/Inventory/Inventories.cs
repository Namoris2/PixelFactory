using Godot;
using System;

public partial class Inventories : CanvasLayer
{
	World tileMap;
	dynamic buildingDisplayInfo;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tileMap = GetNode<World>("/root/main/World/TileMap");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Back") && Visible)
		{
			tileMap.UITOGGLE = false;
			tileMap.EmitSignal("ToggleInventory", tileMap.UITOGGLE, Newtonsoft.Json.JsonConvert.SerializeObject(buildingDisplayInfo));
		}
	}

    public override void _Input(InputEvent @event)
    {
        
		if (@event.IsActionPressed("Interact"))
		{
			buildingDisplayInfo = tileMap.GetBuildingInfo(tileMap.cellPostionByMouse);
			
			if (!Visible && tileMap.buildingsData != null)
			{
				tileMap.UITOGGLE = true;
			}
			else if (Visible)
			{
				tileMap.UITOGGLE = false;
			}

			tileMap.EmitSignal("ToggleInventory", tileMap.UITOGGLE, Newtonsoft.Json.JsonConvert.SerializeObject(buildingDisplayInfo));
		}

    }
    

	public void ToggleInventory(bool TOGGLEINGINVENTORY)
	{
		if (TOGGLEINGINVENTORY)
		{
			this.Show();
		}
		else
		{
			this.Hide();
		}
	}
}
