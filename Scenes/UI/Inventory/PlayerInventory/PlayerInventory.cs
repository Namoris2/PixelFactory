using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

public partial class PlayerInventory : Control
{
    FlowContainer flowContainer;
	private InventorySlot inventorySlot;
	private string inventorySlotPath = "res://Scenes/UI/Inventory/InventorySlot.tscn";
	public InventorySlot[] inventorySlots;
	dynamic items;
	
	[Export]
	private int inventorySize = 0; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		inventorySlots = new InventorySlot[inventorySize];

		flowContainer = GetNode<FlowContainer>("FlowContainer");
		CreateInventorySlots();
		items = inventorySlots[0].items;

		LoadingScreen loadingScreen = GetNodeOrNull<LoadingScreen>("/root/LoadingScreen");

		// cheat items to inventory
		/*if (loadingScreen == null || !loadingScreen.loadingSave)
		{
			PutToInventory("IronIngot", 100);
			PutToInventory("IronPlate", 100);
			PutToInventory("IronRod", 100);

			/*PutToInventory("CopperIngot", 2000);
			PutToInventory("IronIngot", 100);
			PutToInventory("CopperIngot", 100);
			PutToInventory("IronPlate", 200);
			PutToInventory("IronRod", 200);
			PutToInventory("Wire", 500);
		}*/
	}

	private void CreateInventorySlots()
	{
		for (int i = 0; i < inventorySize; i++)
		{
			inventorySlot = (InventorySlot)GD.Load<PackedScene>(inventorySlotPath).Instantiate();
			inventorySlot.Name = $"Slot{i}";
			inventorySlot.inventoryType = "inventory";
			flowContainer.AddChild(inventorySlot);

			inventorySlots[i] = inventorySlot;
		}
	}

	public int PutToInventory(string itemType, int amount)
	{
		int maxStackSize = (int)items[itemType].maxStackSize;
		if (amount == 0) { return 0; }
		for (int i = 0; i < inventorySlots.Length; i++)
		{
			Label amountLabel = inventorySlots[i].resourceAmount;

			if (inventorySlots[i].itemType == itemType)
			{
				int slotAmount = int.Parse(amountLabel.Text);
				if (slotAmount + amount <= maxStackSize)
				{
					amountLabel.Text = (amount + slotAmount).ToString();
					return 0;
				}
				else if (slotAmount + amount >= maxStackSize)
				{
					amountLabel.Text = items[itemType].maxStackSize.ToString();
					amount = amount + slotAmount - maxStackSize;
				}
			}
		}
		
		for (int i = 0; i < inventorySlots.Length; i++)
		{
			Label amountLabel = inventorySlots[i].resourceAmount;

			if (inventorySlots[i].itemType == "")
			{
				int slotAmount = 0;
				if (slotAmount + amount <= maxStackSize)
				{
					inventorySlots[i].itemType = itemType;
					amountLabel.Text = amount.ToString();
					inventorySlots[i].UpdateSlotTexture(itemType);
					return 0;
				}
				else if (slotAmount + amount >= maxStackSize)
				{
					inventorySlots[i].itemType = itemType;
					inventorySlots[i].UpdateSlotTexture(itemType);
					amountLabel.Text = items[itemType].maxStackSize.ToString();
					amount = amount + slotAmount - maxStackSize;
				}
			}
		}
		return amount;
	}
	
	public bool IsInInventory(string itemType, int amount)
	{
		if (itemType == "" || amount == 0) { return true; }

		for (int i = 0; i < inventorySlots.Length; i++)
		{
			if (itemType == inventorySlots[i].itemType)
			{
				int slotAmount = int.Parse(inventorySlots[i].GetNode<Label>("ResourceAmount").Text);
				if (amount <= slotAmount)
				{
					amount = 0;
				}
				else if (amount > slotAmount)
				{
					amount -= slotAmount;
				}

				if (amount == 0) { return true; }
			}
		}
		return false;
	}

	public void RemoveFromInventory(string itemType, int amount)
	{
		if (itemType == "" || amount == 0) { return; }

		for (int i = inventorySlots.Length - 1; i >= 0; i--)
		{
			Label amountLabel = inventorySlots[i].GetNode<Label>("ResourceAmount");
			
			if (itemType == inventorySlots[i].itemType)
			{
				if (amount >= int.Parse(amountLabel.Text))
				{
					amount -= int.Parse(amountLabel.Text);
					amountLabel.Text = "";
					inventorySlots[i].UpdateSlotTexture("");
					inventorySlots[i].itemType = "";
				}
				else 
				{
					amountLabel.Text = (int.Parse(amountLabel.Text) - amount).ToString();
					amount = 0;
				}
			}

			if (amount == 0) { return; }
		}
	}

	public int GetItemAmount(string itemType)
	{
		int amount = 0;
		for (int i = 0; i < inventorySlots.Length; i++)
		{
			Label amountLabel = inventorySlots[i].GetNode<Label>("ResourceAmount");
			
			if (itemType == inventorySlots[i].itemType)
			{
				amount += int.Parse(amountLabel.Text);
			}
		}
		return amount;
	}

	public bool HasSpace(string itemType, int amount)
	{
		if (itemType == "" || amount == 0) { return true; }
		for (int i = 0; i < inventorySlots.Length; i++)
		{
			if (inventorySlots[i].itemType == "")
			{
				amount -= (int)items[itemType].maxStackSize;
			}
			else if (inventorySlots[i].itemType == itemType)
			{
				amount -= (int)items[itemType].maxStackSize - int.Parse(inventorySlots[i].resourceAmount.Text);
			}
			if (amount <= 0)
			{
				return true;
			}
		}
		return false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public List<System.Collections.Generic.Dictionary<string, dynamic>> Save()
	{
		List<System.Collections.Generic.Dictionary<string, dynamic>> slots = new();
		foreach (InventorySlot slot in inventorySlots)
		{
			System.Collections.Generic.Dictionary<string, dynamic> slotInfo = new();
			string itemType = slot.itemType;
			string amountString = slot.GetNode<Label>("ResourceAmount").Text;

			int amount = 0;
			if (itemType != "")
			{
				if (amountString != "")
				{
					amount = int.Parse(amountString);
				}
				else
				{
					amount = 1;
				}
			}

			slotInfo.Add("resource", itemType);
			slotInfo.Add("amount", amount);
			slots.Add(slotInfo);
		}
		return slots;
	}

	public void Load(dynamic data)
	{
		dynamic loadedData = data[Name];
		//GD.Print(loadedData);
		for (int i = 0; i < loadedData.Count; i++)
		{
			string itemType = loadedData[i].resource.ToString();
			int amount = (int)loadedData[i].amount;
			InventorySlot slot = inventorySlots[i];

			if (itemType != "")
			{
				slot.itemType = itemType;
				if (amount > 0)
				{
					slot.GetNode<Label>("ResourceAmount").Text = amount.ToString();
				}
				slot.UpdateSlotTexture(itemType);
			}
		}

		//GD.Print("Inventory Loaded");
	}
}
