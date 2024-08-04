using Godot;
using Godot.Collections;
using System;
using System.IO;
using System.Numerics;

public partial class AnimatedBuildingPartsController : Node2D
{
	string path = $"res://Scenes/Game/World/AnimatedBuildingParts/Anim_";

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

	private void CreateAnimatedBuildingPart(Vector2I coords, string type, int rotation)
	{
		if (!Godot.FileAccess.FileExists($"{path}{type}.tscn")) { return; }

		Array<Node> buildingParts = GetTree().GetNodesInGroup($"Anim_{type}");
		
		Node2D buildingPart;

		if (buildingParts.Count == 0) 
		{ 
			buildingPart = (Node2D)GD.Load<PackedScene>($"{path}{type}.tscn").Instantiate();
		}
		else
		{
			Node existingBuildingPart = buildingParts[0];
			buildingPart = (Node2D)existingBuildingPart.Duplicate();
			AnimationPlayer currentAnimation = existingBuildingPart.GetNode<AnimationPlayer>("AnimationPlayer");
			AnimationPlayer newAnimation = buildingPart.GetNode<AnimationPlayer>("AnimationPlayer");

			newAnimation.Play("Main");
			newAnimation.Advance(currentAnimation.CurrentAnimationPosition);
		}

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
