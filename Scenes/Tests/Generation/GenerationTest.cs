using Godot;
using System;
using System.Globalization;

public partial class GenerationTest : TileMap
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GenerateWorld generateWorld = GetNode<GenerateWorld>("/root/main/GenerateWorld");

		/*int mapRadius = 250;
		generateWorld.GenerateResource(this, mapRadius, "Grass", true);
		generateWorld.GenerateResource(this, mapRadius, "IronOre");
		generateWorld.GenerateResource(this, mapRadius, "CoalOre");
		generateWorld.GenerateResource(this, mapRadius, "CopperOre");*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
