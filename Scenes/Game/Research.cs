using Godot;
using System;
using System.Collections.Generic;

public partial class Research : Node
{
	public static List<string> research;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		research = new() { "Tutorial0" };
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Load(dynamic data) 
    {
        dynamic loadedData = data[Name];
       
	    if (loadedData != null) 
		{ 
			for (int i = 1; i < loadedData.Count; i++)
			{
				research.Add(loadedData[i].ToString());
			}
		}

		for (int i = 0; i < research.Count; i++)
		{
			GetTree().CallGroup("ResearchItemContainer", "UnlockItem", research[i]);
		}

    }

    public List<string> Save()
    {
        return research;
    }

	public void UnlockResearch(string unlock)
	{
		research.Add(unlock);
		GetTree().CallGroup("ResearchItemContainer", "UnlockItem", unlock);
	}
}
