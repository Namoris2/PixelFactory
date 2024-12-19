using Godot;
using System;

public partial class GameEvents : Node
{
	private World tileMap;
    private Inventories inventories;
    private BuildMenu buildMenu;
    private PauseMenu pauseMenu;
    public Leftovers leftovers;
    private Label worldInfo;
    public static PlayerCamera camera;

    public static ActionInfoPopup pickUpItemPopup;
    public static ActionInfoPopup closePopup;
    public static ActionInfoPopup toggleInventoryPopup;
    public static ActionInfoPopup toggleBuildingInventoryPopup;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        tileMap = GetNode<World>("../World/TileMap");
        inventories = GetNode<Inventories>("../UI/Inventories");
        buildMenu = GetNode<BuildMenu>("../UI/BuildMenu");
        pauseMenu = GetNode<PauseMenu>("../UI/PauseMenu");
        worldInfo = GetNode<Label>("../UI/WorldInfo");
        camera = GetNode<PlayerCamera>("../World/Player/PlayerCamera");

        pickUpItemPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("PickUpItem");
        closePopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("Back");
        toggleInventoryPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("ToggleInventory");
        toggleBuildingInventoryPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("ToggleBuildingInventory");
	}

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Back"))
        {
            if (pauseMenu.Visible)
            {
                pauseMenu.UnpauseGame();
                closePopup.Hide();
                toggleInventoryPopup.Show();
            }
            else if (!(tileMap.BUILDINGMODE || tileMap.DISMANTLEMODE || inventories.Visible || buildMenu.Visible))
            {
                pauseMenu.PauseGame();
                closePopup.SetDefaultActionText();
                closePopup.Show();
                pickUpItemPopup.Hide();
                toggleInventoryPopup.Hide();
                toggleBuildingInventoryPopup.Hide();
            }
            else if (tileMap.DISMANTLEMODE)
            {
                tileMap.ToggleDismantleMode(false);
            }
            else if (buildMenu.Visible)
            {
                camera.toggleZoom = true;
                buildMenu.ToggleBuildMode();
            }
            else if (inventories.Visible)
            {
                inventories.ToggleInventory(false);
                ToggleBuildingInventory(false);
            }
        }

        if (!pauseMenu.Visible)
        {
            if (@event.IsActionPressed("ToggleInventory"))
            {
                if (!buildMenu.Visible)
                {
                    inventories.ToggleInventory(!inventories.Visible);
                    camera.toggleZoom = !inventories.Visible;    
                    tileMap.UITOGGLE = inventories.Visible;
                    worldInfo.Visible = !inventories.Visible;
                }
            }

            if (@event.IsActionPressed("Interact"))
            {
                ToggleBuildingInventory(!inventories.Visible);
            }

            if (@event.IsActionPressed("ToggleBuildMode"))
            {
                buildMenu.ToggleBuildMode();
                camera.toggleZoom = !buildMenu.Visible;
            }
        }
    }

    private void ToggleBuildingInventory(bool toggle)
    {
        camera.toggleZoom = !toggle;
        if (leftovers != null) // Open leftovers Inventory
        {
            inventories.ToggleBuildingInventory(toggle, "", leftovers);
        }
        else // Open building Inventory
        {
            inventories.ToggleBuildingInventory(toggle, tileMap.GetBuildingInfo(tileMap.cellPositionByMouse));
        }
    }
}
