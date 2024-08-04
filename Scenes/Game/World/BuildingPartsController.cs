using Godot;
using System;
using System.IO;
using System.Numerics;


public partial class BuildingPartsController : Node2D
{
	string path = $"res://Scenes/Game/World/BuildingParts/";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		World world = GetNode<World>("../TileMap");
        world.CreateBuildingPart += CreateBuildingPart;
        world.RemoveBuildingPart += RemoveBuildingPart;
	}



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void CreateBuildingPart(Vector2I coords, string type, int rotation)
	{
		if (!Godot.FileAccess.FileExists($"{path}{type}.tscn")) { return; }

		Node2D buildingPart = (Node2D)GD.Load<PackedScene>($"{path}{type}.tscn").Instantiate();
		buildingPart.Position = coords * 64;
		buildingPart.Name = $"{type}{coords[0]}x{coords[1]}";
		AddChild(buildingPart);
	}

	private void RemoveBuildingPart(Vector2I coords, string type)
	{
		Node buildingPart = GetNodeOrNull<Node>($"{type}{coords[0]}x{coords[1]}");
		if (buildingPart != null) { buildingPart.QueueFree(); }
	}
}
