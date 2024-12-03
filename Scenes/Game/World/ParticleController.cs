using Godot;
using System;

public partial class ParticleController : Node2D
{
    string path = $"res://Particles/Buildings/";
    public override void _Ready()
    {
        World world = GetNode<World>("../TileMap");
        world.CreateParticle += CreateParticle;
        world.RemoveParticle += RemoveParticle;
    }

    private void CreateParticle(Vector2I coords, string type, string resource = "")
    {
        if (resource != "") { resource = "/" + resource; }
		if (!ResourceLoader.Exists($"{path}{type}{resource}.tscn")) { return; }

        Node2D particle = (Node2D)GD.Load<PackedScene>($"{path}{type}{resource}.tscn").Instantiate();
        particle.Position = coords * 64;
        particle.Name = $"{type}{coords[0]}x{coords[1]}";
        AddChild(particle);
    }

    private void RemoveParticle(Vector2I coords, string type)
    {
		Node particle = GetNodeOrNull<Node>($"{type}{coords[0]}x{coords[1]}");
        if (particle != null) { particle.QueueFree(); }
    }
}
