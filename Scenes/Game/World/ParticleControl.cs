using Godot;
using System;

public partial class ParticleControl : Node2D
{
    public override void _Ready()
    {
        World world = GetNode<World>("../TileMap");
        world.CreateParticle += CreateParticle;
        world.RemoveParticle += RemoveParticle;
    }

    private void CreateParticle(Vector2I coords, string type, string resource)
    {
        switch (type)
        {
            case "smallDrill":
            	Node2D particle = (Node2D)GD.Load<PackedScene>($"res://Particles/Buildings/SmallDrill/Drilling{resource}.tscn").Instantiate();
                Vector2 particlePosition = coords + new Vector2(0.5f, 10f / 16);
                particle.Position = particlePosition * 64;
                particle.Name = $"DrillParticles{coords[0]}x{coords[1]}";
                AddChild(particle);
                break;
        }
    }

    private void RemoveParticle(Vector2I coords)
    {
		Node particle = GetNodeOrNull<Node>($"DrillParticles{coords[0]}x{coords[1]}");

        if (particle != null) { particle.QueueFree(); }
    }
}
