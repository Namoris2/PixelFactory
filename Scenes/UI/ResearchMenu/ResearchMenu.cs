using Godot;
using System;

public partial class ResearchMenu : Control
{
	PlayerInventory playerInventory;
	Research researchController;
	public HBoxContainer researchSelects;
	HFlowContainer unlocksContainer;
	HFlowContainer costContainer;
	Label researchLabel;
	Label researchedLabel;
	Button researchButton;

	public string selectedResearch;
	dynamic unlocks;
	//dynamic
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playerInventory = (PlayerInventory)GetTree().GetFirstNodeInGroup("PlayerInventory");
		researchController = (Research)GetTree().GetFirstNodeInGroup("Research");
		researchSelects = GetNode<HBoxContainer>("ScrollContainer/ResearchSelects");
		unlocksContainer = GetNode<HFlowContainer>("HSplitContainer/PanelContainer/Unlocks");
		costContainer = GetNode<HFlowContainer>("HSplitContainer/ResearchCost/CostContainer");
		researchLabel = GetNode<Label>("HSplitContainer/ResearchCost/ResearchName");
		researchedLabel = GetNode<Label>("HSplitContainer/ResearchCost/Researched");
		researchButton = GetNode<Button>("HSplitContainer/ResearchCost/Research");

		unlocks = LoadFile.LoadJson("unlocks.json");
		researchButton.Pressed += ResearchItems;

		// Hides all research selects
		for (int i = 2; i < researchSelects.GetChildCount() - 1; i++)
		{
			(researchSelects.GetChild(i) as Button).Hide();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ToggleResearchMenu(bool? toggle = null)
	{
		if (toggle == null)
		{
			toggle = !Visible;
		}

		Visible = (bool)toggle;
		if (!GameEvents.tileMap.BUILDINGMODE && !GameEvents.tileMap.DISMANTLEMODE) { GameEvents.closePopup.Visible = !GameEvents.closePopup.Visible; }

		if (Visible)
		{
			CheckResearchCost();
			GameEvents.toggleBuildingInventoryPopup.Hide();
			GameEvents.toggleInventoryPopup.Hide();
			GameEvents.toggleBuildMenuPopup.Hide();
			GameEvents.toggleDismantleModePopup.Hide();
			GameEvents.rotatePopup.Hide();
			GameEvents.toggleResearchMenuPopup.SetCustomActionText();
		}
		else
		{
			GameEvents.toggleBuildingInventoryPopup.Show();
			GameEvents.toggleInventoryPopup.Show();
			GameEvents.toggleBuildMenuPopup.Show();
			GameEvents.toggleDismantleModePopup.Show();
			GameEvents.toggleResearchMenuPopup.SetDefaultActionText();

			if (GameEvents.tileMap.BUILDINGMODE && (bool)GameEvents.tileMap.buildings[GameEvents.tileMap.selectedBuilding].canRotate)
			{
				GameEvents.rotatePopup.Show();
			}
		}
	}

	public void ChangeTab(string researchName, string researchText)
	{
		selectedResearch = researchName;
		dynamic research = unlocks[researchName];

		foreach (Control slot in costContainer.GetChildren())
		{
			slot.Hide();
		}

		foreach (Control unlock in unlocksContainer.GetChildren())
		{
			unlock.QueueFree();
		}

		researchedLabel.Hide();
		researchButton.Show();


		researchLabel.Text = researchText;
		if (Research.research.Contains(researchName))
		{
			researchedLabel.Show();
			researchButton.Hide();
		}

		for (int i = 0; i < research.unlocks.Count; i++)
		{
			AddResearchInfo(research.unlocks[i], researchName);
		}

		for (int i = 0; i < research.researchUnlocks.Count; i++)
		{
			AddResearchInfo(research.researchUnlocks[i], researchName, "research");
		}

		CheckResearchCost();
	}

	private void AddResearchInfo(dynamic researchInfo, string researchName, string unlockType = "")
	{
		string unlockName = "";
		string jsonName = "";
		string name = "";
		if (unlockType == "")
		{ 
			unlockType = researchInfo.type.ToString(); 

			if (unlockType != "inventorySlots")
			{
				unlockName = researchInfo.name.ToString();
				jsonName = $"{unlockType}s.json";
				name = LoadFile.LoadJson(unlockType + "s.json")[unlockName].name.ToString();
			}
		}

		if (unlockType == "recipe")
		{
			jsonName = "items.json";
		}

		Control unlock = (Control)GD.Load<PackedScene>("Scenes/UI/ResearchMenu/Unlock.tscn").Instantiate();
		TextureRect unlockIcon = unlock.GetNode<TextureRect>("Icon");
		Label unlockLabel = unlock.GetNode<Label>("Name");

		if (unlockType == "research")
		{
			unlockIcon.Texture = GD.Load<Texture2D>("res://Gimp/Icons/ResearchIcons/NewResearch.png");
			unlockLabel.Text = $"New research: {researchSelects.GetNode<ResearchSelect>(researchInfo.ToString()).Text}";
		}
		else if (unlockType == "inventorySlots")
		{
			unlockIcon.Texture = GD.Load<Texture2D>("res://Gimp/Icons/ResearchIcons/InventorySlot.png");
			unlockLabel.Text = $"+ {researchInfo.amount} Inventory Slots";
		}
		else
		{
			unlockIcon.Texture = main.GetTexture(jsonName, unlockName);
			unlockLabel.Text = $"{unlockType[0].ToString().ToUpper() + unlockType.Substring(1)}: {name}";
		}

		if (Research.research.Contains(researchName))
		{
			unlockIcon.Modulate = new("#8c8c8c");
			unlockLabel.Modulate = new("#8c8c8c");
		}

		unlocksContainer.AddChild(unlock);
	}

	public void CheckResearchCost()
	{
		bool canResearch = true;
		dynamic research = unlocks[selectedResearch];

		for (int i = 0; i < research.researchCost.Count; i++)
		{
			int amount = playerInventory.GetItemAmount(research.researchCost[i].name.ToString());
			if (!Research.research.Contains(selectedResearch)) { ShowCostSlot(research.researchCost[i], i); } // Sets information to all cost slots
			canResearch &= amount >= (int)research.researchCost[i].amount;
		}

		researchButton.Disabled = !canResearch;
	}

	private void ShowCostSlot(dynamic cost, int index)
	{
		string costName = cost.name.ToString();
		int costAmount = (int)cost.amount;

		InventorySlot slot = (InventorySlot)costContainer.GetChild(index);
		int amount = playerInventory.GetItemAmount(costName);
		slot.resourceAmount.Text = $"{amount}/{costAmount}";
		slot.itemType = costName;
		slot.UpdateSlotTexture(costName);
		slot.Show();
	}

	private void RemoveCostItems()
	{
		dynamic research = unlocks[selectedResearch];

		for (int i = 0; i < research.researchCost.Count; i++)
		{
			playerInventory.RemoveFromInventory(research.researchCost[i].name.ToString(), (int)research.researchCost[i].amount);
		}
	}

	private void ResearchItems()
	{
		researchedLabel.Show();
		researchButton.Hide();

		foreach (Control slot in costContainer.GetChildren())
		{
			slot.Hide();
		}

		foreach (Control unlock in unlocksContainer.GetChildren())
		{
			unlock.GetNode<TextureRect>("Icon").Modulate = new("#8c8c8c");
			unlock.GetNode<Label>("Name").Modulate = new("#8c8c8c");
		}

		researchController.UnlockResearch(selectedResearch);
		RemoveCostItems();
		ShowUnlockedResearch(selectedResearch);
	}

	public void ShowUnlockedResearch(string researchName)
	{
		foreach (dynamic researchUnlock in unlocks[researchName].researchUnlocks)
		{
			researchSelects.GetNode<Button>(researchUnlock.ToString()).Show();
		}

		foreach (dynamic unlock in unlocks[researchName].unlocks)
		{
			if (unlock.type.ToString() == "inventorySlots")
			{
				playerInventory.inventorySize += (int)unlock.amount;
				playerInventory.AddInventorySlots((int)unlock.amount);
			}
		}
	}
}
