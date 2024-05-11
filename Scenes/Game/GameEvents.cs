using Godot;
using System;

public partial class GameEvents : Node
{
	private World tileMap;
    private Inventories inventories;
    private BuildMenu buildMenu;
    private PauseMenu pauseMenu;
    public Leftovers leftovers;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        tileMap = GetNode<World>("../World/TileMap");
        inventories = GetNode<Inventories>("../UI/Inventories");
        buildMenu = GetNode<BuildMenu>("../UI/BuildMenu");
        pauseMenu = GetNode<PauseMenu>("../UI/PauseMenu");
	}

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Back"))
        {
            if (pauseMenu.Visible)
            {
                pauseMenu.UnpauseGame();
            }
            else if (!(tileMap.BUILDINGMODE || tileMap.DISMANTLEMODE || inventories.Visible || buildMenu.Visible))
            {
                pauseMenu.PauseGame();
            }
            else if (tileMap.DISMANTLEMODE)
            {
                tileMap.ToggleDismantleMode(false);
            }
            else if (buildMenu.Visible)
            {
                buildMenu.ToggleBuildMode();
            }
            else if (inventories.Visible)
            {
                inventories.ToggleInventory(false);
                tileMap.UITOGGLE = false;
            }
        }

        if (!pauseMenu.Visible)
        {
            if (@event.IsActionPressed("ToggleInventory"))
            {
                if (!buildMenu.Visible)
                {
                    inventories.ToggleInventory(!inventories.Visible);
                    tileMap.UITOGGLE = inventories.Visible;
                }
            }
            if (@event.IsActionPressed("Interact"))
            {
                tileMap.UITOGGLE = !tileMap.UITOGGLE;
                if (leftovers != null)
                {
                    inventories.ToggleBuildingInventory(!inventories.Visible, "", leftovers);
                }
                else
                {
                    inventories.ToggleBuildingInventory(!inventories.Visible, tileMap.GetBuildingInfo(tileMap.cellPositionByMouse));
                }
            }
        }
    }
}
