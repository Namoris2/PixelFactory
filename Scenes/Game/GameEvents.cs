using Godot;
using System;

public partial class GameEvents : Node
{
	public static World tileMap;
    private Inventories inventories;
    private BuildMenu buildMenu;
    private ResearchMenu researchMenu;
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
    public static ActionInfoPopup toggleResearchMenuPopup;
    public static ActionInfoPopup splitStackPopup;

    public static CollectedItemsContainer collectedItemsContainer;
    public static TabContainer tutorialController;
    public static Control tutorialContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        tileMap = GetNode<World>("../World/TileMap");
        inventories = GetNode<Inventories>("../UI/Inventories");
        buildMenu = GetNode<BuildMenu>("../UI/Menus/BuildMenu");
        researchMenu = GetNode<ResearchMenu>("../UI/Menus/ResearchMenu");
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
        toggleResearchMenuPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("ToggleResearchMenuPopup");
        splitStackPopup = (ActionInfoPopup)GetTree().GetFirstNodeInGroup("SplitStackPopup");

        collectedItemsContainer = GetNode<CollectedItemsContainer>("../UI/CollectedItemsContainer");
        tutorialController = (TabContainer)GetTree().GetFirstNodeInGroup("TutorialContainer");
        tutorialContainer = (Control)tutorialController.GetParent();
	}

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Back"))
        {
            if (pauseMenu.Visible)
            {
                pauseMenu.UnpauseGame();
            }
            else if (!(tileMap.BUILDINGMODE || tileMap.DISMANTLEMODE || inventories.Visible || buildMenu.Visible || researchMenu.Visible))
            {
                pauseMenu.PauseGame();
            }
            else if (tileMap.DISMANTLEMODE && !buildMenu.Visible && !inventories.Visible && !researchMenu.Visible)
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
                if (IsInstanceValid(tutorialContainer)) { tutorialContainer.Show(); }
            }
            else if (inventories.Visible)
            {
                inventories.ToggleInventory(false);
                ToggleBuildingInventory(false);
            }
            else if (researchMenu.Visible)
            {
                researchMenu.ToggleResearchMenu(false);
                tileMap.UITOGGLE = false;
                camera.toggleZoom = true;
            }
        }

        if (!pauseMenu.Visible)
        {
            if (@event.IsActionPressed("ToggleInventory"))
            {
                if (!buildMenu.Visible && !researchMenu.Visible)
                {
                    inventories.ToggleInventory(!inventories.Visible);
                    camera.toggleZoom = !inventories.Visible;    
                    tileMap.UITOGGLE = inventories.Visible;
                    worldInfo.Visible = !inventories.Visible;
                }
            }

            if (@event.IsActionPressed("Interact"))
            {
                if (!buildMenu.Visible && !researchMenu.Visible)
                {
                    ToggleBuildingInventory(!inventories.Visible);
                }
            }

            if (@event.IsActionPressed("ToggleBuildMode"))
            {
                if (!inventories.Visible && !researchMenu.Visible/*&& Research.research.Count > 1*/)
                {                
                    buildMenu.ToggleBuildMode();
                    camera.toggleZoom = !buildMenu.Visible;
                    if (!tileMap.BUILDINGMODE && !tileMap.DISMANTLEMODE) { closePopup.Visible = !closePopup.Visible; }
                    toggleInventoryPopup.Visible = !toggleInventoryPopup.Visible;
                    toggleBuildingInventoryPopup.Hide();
                    rotatePopup.Hide();
                    toggleDismantleModePopup.Visible = !toggleDismantleModePopup.Visible;
                    toggleResearchMenuPopup.Visible = !toggleResearchMenuPopup.Visible;
                    if (IsInstanceValid(tutorialContainer)) { tutorialContainer.Visible = !tutorialContainer.Visible; }

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

            if (@event.IsActionPressed("ToggleResearchMenu"))
            {
                if (!inventories.Visible && !buildMenu.Visible)
                {
                    researchMenu.ToggleResearchMenu();
                    camera.toggleZoom = !researchMenu.Visible;
                    tileMap.UITOGGLE = researchMenu.Visible;
                    worldInfo.Visible = !researchMenu.Visible;
                }
            }
        }
    }

    private void ToggleBuildingInventory(bool toggle)
    {
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
