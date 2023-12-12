using Godot;
using System;

public partial class GenerateWorld : Node
{
    public void GenerateResource(Vector2I mapSize, string resourceInput, bool generateWater = false)
	{
        LoadFile load = GetNode<LoadFile>("../LoadFile");
		dynamic groundResources = load.LoadJson("groundResources.json");
		
        dynamic resource = groundResources[resourceInput];
		float generationCap = (float)resource.generationCap;
		Vector2I atlasCoords = new Vector2I((int)resource.atlasCoords[0], (int)resource.atlasCoords[1]);

		FastNoiseLite noise = new FastNoiseLite();
		Random rnd = new Random();
		noise.Seed = rnd.Next() + (int)resource.addedSeed;
		noise.Frequency = (float)resource.frequency;

		TileMap tileMap = GetNode<TileMap>("../World/TileMap");

		for (int x = 0; x < mapSize.X; x++)
		{
			for (int y = 0; y < mapSize.Y; y++)
			{
				float noiseValue = noise.GetNoise2D(x, y);
				

				if (generationCap > noiseValue)
				{
					if (generateWater)
					{
						tileMap.SetCell(0, new Vector2I(x, y), 0, atlasCoords);
					}				
					else if ((string)tileMap.GetCellTileData(0, new Vector2I(x, y)).GetCustomData("resourceName") == "Grass")
					{
						tileMap.SetCell(0, new Vector2I(x, y), 0, atlasCoords);
					}
				}
				else if (generateWater)
				{
					tileMap.SetCell(0, new Vector2I(x, y), 0, new Vector2I(4, 0));
				}
			}
		}
	}
}
