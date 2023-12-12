using Godot;
using System;
using System.Globalization;

public partial class GenerationTest : TileMap
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadFile load = GetNode<LoadFile>("../LoadFile");
		dynamic groundResources = load.LoadJson("groundResources.json");


		Vector2I mapSize = new Vector2I(500, 500);
		GenerateResource(mapSize, groundResources.Grass, true);
		GenerateResource(mapSize, groundResources.IronOre);
		GenerateResource(mapSize, groundResources.CoalOre);
		GenerateResource(mapSize, groundResources.CopperOre);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void GenerateResource(Vector2I mapSize, dynamic resource, bool generateWater = false)
	{
		float generationCap = (float)resource.generationCap;
		Vector2I atlasCoords = new Vector2I((int)resource.atlasCoords[0], (int)resource.atlasCoords[1]);

		FastNoiseLite noise = new FastNoiseLite();
		Random rnd = new Random();
		noise.Seed = rnd.Next();
		noise.Frequency = (float)resource.frequency;

		for (int x = 0; x < mapSize.X; x++)
		{
			for (int y = 0; y < mapSize.Y; y++)
			{
				float noiseValue = noise.GetNoise2D(x, y);
				

				if (generationCap > noiseValue)
				{
					if (generateWater)
					{
						SetCell(0, new Vector2I(x, y), 0, atlasCoords);
					}				
					else if ((string)GetCellTileData(0, new Vector2I(x, y)).GetCustomData("resourceName") == "Grass")
					{
						SetCell(0, new Vector2I(x, y), 0, atlasCoords);
					}
				}
				else if (generateWater)
				{
					SetCell(0, new Vector2I(x, y), 0, new Vector2I(4, 0));
				}
			}
		}
	}
}
