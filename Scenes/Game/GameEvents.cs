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
    public static ActionInfoPopup rotatePopup;
    public static ActionInfoPopup harvestResourcePopup;
    public static ActionInfoPopup toggleInventoryPopup;
    public static ActionInfoPopup toggleBuildingInventoryPopup;
    public static ActionInfoPopup toggleBuildMenuPopup;
    public static ActionInfoPopup toggleDismantleModePopup;
    public static ActionInfoPopup splitStackPopup;

    public static CollectedItemsContainer collectedItemsContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        tileMap = GetNode<World>("../World/TileMap");
        inventories = GetNode<Inventories>("../UI/Inventories");
        buildMenu = GetNode<BuildMenu>("../UI/BuildMenu");
        pauseMenu = GetNode<PauseMenu>("../UI/PauseMenu");
        worldInfo = GetNode<Label>("../UI/WorldInfo");
        camera = GetNode<PlayerCamera>("../World/Player/PlayerCamera");

        closePopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("BackPopup");
        rotatePopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("RotateBuildingPopup");
        pickUpItemPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("PickUpItemPopup");
        harvestResourcePopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("HarvestResourcePopup");
        toggleInventoryPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("ToggleInventoryPopup");
        toggleBuildingInventoryPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("ToggleBuildingInventoryPopup");
        toggleBuildMenuPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("ToggleBuildMenuPopup");
        toggleDismantleModePopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("ToggleDismantleModePopup");
        splitStackPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("SplitStackPopup");

        collectedItemsContainer = GetNode<CollectedItemsContainer>("../UI/CollectedItemsContainer");
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
            else if (tileMap.DISMANTLEMODE && !buildMenu.Visible)
            {
                tileMap.ToggleDismantleMode(false);
            }
            else if (buildMenu.Visible)
            {
                camera.toggleZoom = true;
                buildMenu.ToggleBuildMode();
                if (!tileMap.DISMANTLEMODE) { closePopup.Hide(); }
                toggleBuildMenuPopup.SetDefaultActionText();
                toggleInventoryPopup.Show();
                toggleDismantleModePopup.Show();
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
                if (!buildMenu.Visible)
                {
                    ToggleBuildingInventory(!inventories.Visible);
                }
            }

            if (@event.IsActionPressed("ToggleBuildMode"))
            {
                if (!inventories.Visible /*&& Research.research.Count > 1*/)
                {                
                    buildMenu.ToggleBuildMode();
                    camera.toggleZoom = !buildMenu.Visible;
                    if (!tileMap.BUILDINGMODE && !tileMap.DISMANTLEMODE) { closePopup.Visible = !closePopup.Visible; }
                    toggleInventoryPopup.Visible = !toggleInventoryPopup.Visible;
                    toggleBuildingInventoryPopup.Hide();
                    rotatePopup.Hide();
                    toggleDismantleModePopup.Visible = !toggleDismantleModePopup.Visible;

                    // Build Menu is closed
                    if (toggleBuildMenuPopup.actionText == toggleBuildMenuPopup.GetNode<Label>("Label").Text)
                    {
                        toggleBuildMenuPopup.SetCustomActionText();
                    }
                    // Build Menu is opened
                    else
                    {
                        toggleBuildMenuPopup.SetDefaultActionText();
                    }
                }
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
