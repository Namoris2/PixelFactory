using Godot;
using System;

public partial class Item : Resource
{
    [Export]
    public int ID { get; set; }
    [Export]
    public string name { get; set; }
    [Export]
    public string reourcePath { get; set; }
    [Export]
    public int resourceAmount { get; set; }
    [Export]
    public int stackSize { get; set; }
    [Export]
    public bool isStackable { get; set; }
}
