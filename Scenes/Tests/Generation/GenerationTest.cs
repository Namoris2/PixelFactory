using Godot;
using System;
using System.Globalization;

public partial class GenerationTest : TileMap
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GenerateWorld generateWorld = GetNode<GenerateWorld>("/root/main/GenerateWorld");

		int mapRadius = 250;
		generateWorld.GenerateResource(mapRadius, "Grass", true);
		generateWorld.GenerateResource(mapRadius, "IronOre");
		generateWorld.GenerateResource(mapRadius, "CoalOre");
		generateWorld.GenerateResource(mapRadius, "CopperOre");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
