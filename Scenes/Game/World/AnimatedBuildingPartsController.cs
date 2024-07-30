using Godot;
using System;
using System.IO;
using System.Numerics;

public partial class AnimatedBuildingPartsController : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		World world = GetNode<World>("../TileMap");
        world.CreateAnimatedBuildingPart += CreateAnimatedBuildingPart;
        world.RemoveAnimatedBuildingPart += RemoveAnimatedBuildingPart;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void CreateAnimatedBuildingPart(Vector2I coords, string type)
	{
		if (!Godot.FileAccess.FileExists($"res://Scenes/Game/World/BuildingParts/Anim_{type}.tscn")) { return; }

		Node2D buildingPart = (Node2D)GD.Load<PackedScene>($"res://Scenes/Game/World/BuildingParts/Anim_{type}.tscn").Instantiate();
		buildingPart.Position = coords * 64;
		buildingPart.Name = $"{type}{coords[0]}x{coords[1]}";
		AddChild(buildingPart);
	}

	private void RemoveAnimatedBuildingPart(Vector2I coords, string type)
	{
		Node buildingPart = GetNodeOrNull<Node>($"{type}{coords[0]}x{coords[1]}");
		if (buildingPart != null) { buildingPart.QueueFree(); }
	}
}
