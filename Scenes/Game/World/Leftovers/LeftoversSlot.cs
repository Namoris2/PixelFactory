using Godot;
using System;
using System.Dynamic;

public partial class LeftoversSlot
{
    public string itemType { get; set; }
    public int itemAmount { get; set; }

    public LeftoversSlot(string _itemType, int _itemAmount)
    {
        itemType = _itemType;
        itemAmount = _itemAmount;
    }
}