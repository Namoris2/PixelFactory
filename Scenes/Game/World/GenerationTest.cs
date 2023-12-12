using Godot;
using System;
using System.Globalization;

public partial class GenerationTest : TileMap
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GenerateWorld generateWorld = GetNode<GenerateWorld>("/root/main/GenerateWorld");

		Vector2I mapSize = new Vector2I(500, 500);
		generateWorld.GenerateResource(mapSize, "Grass", true);
		generateWorld.GenerateResource(mapSize, "IronOre");
		generateWorld.GenerateResource(mapSize, "CoalOre");
		generateWorld.GenerateResource(mapSize, "CopperOre");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
