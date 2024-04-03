using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GenerateWorld : Node
{
	string[] groundTerrains = { "Grass", "Water", "IronOre", "CopperOre" };

    public int GenerateResource(World tileMap, int mapRadius, int seed, string resourceInput ,bool generateWater = false)
	{
		//GD.Print(tileMap.GetPath());
        LoadFile load = new();
		dynamic groundResources = load.LoadJson("groundResources.json");
		
        dynamic resource = groundResources[resourceInput];
		float generationCap = (float)resource.generationCap;
		//Vector2I atlasCoords = new Vector2I((int)resource.atlasCoords[0], (int)resource.atlasCoords[1]);

        FastNoiseLite noise = new()
        {
            Seed = seed + (int)resource.addedSeed,
            Frequency = (float)resource.frequency
        };

        List<Vector2I> resourceCells = new();
		List<Vector2I> waterCells = new();

		for (int x = mapRadius * -1; x < mapRadius; x++)
		{
			for (int y = mapRadius * -1; y < mapRadius; y++)
			{
				float noiseValue = noise.GetNoise2D(x, y);

				if (generationCap > noiseValue)
				{
					/*if (generateWater)
					{
						waterCells.Add(new Vector2I(x, y));
					}*/
					if (resourceInput == "Grass")
					{
						Vector2I atlasCoords = new Vector2I((int)resource.atlasCoords[0], (int)resource.atlasCoords[1]);
						tileMap.SetCell(0, new Vector2I(x, y), 0, atlasCoords);
					}
					else if ((string)tileMap.GetCellTileData(0, new Vector2I(x, y)).GetCustomData("resourceName") == "Grass")
					{
						resourceCells.Add(new Vector2I(x, y));
					}
				}
				else if (generateWater)
				{
                    waterCells.Add(new Vector2I(x, y));
				}
			}
		}

		//GD.Print(resourceCells.Count);
		Array<Vector2I> resourceCellsArray = new (resourceCells);
		Array<Vector2I> waterCellsArray = new (waterCells);
		
		tileMap.SetCellsTerrainConnect(0, resourceCellsArray, 0, System.Array.IndexOf(groundTerrains, resourceInput));
		if (generateWater)
		{
            tileMap.SetCellsTerrainConnect(0, waterCellsArray, 0, 1);
		}
		return seed;
	}
}
