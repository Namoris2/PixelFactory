using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


public partial class GenerateWorld : Node
{

	string[] groundTerrains = { "Grass", "Water", "IronOre", "CopperOre" };
	public int chunkSize = 16;
	int generationHeight = 9;
	int generationWidth = 13;

	World world;
	int seed;
	string resourceInput;
	dynamic resource;
	Vector2I playerPosition;
	bool generateWater;
	FastNoiseLite noise;


	public List<Vector2I> grassChunks = new();
	public List<Vector2I> waterChunks = new();
	public List<Vector2I> ironChunks = new();
	public List<Vector2I> copperChunks = new();

    public void GenerateResource(World _world, int _seed, string _resourceInput, Vector2I _playerPosition, bool _generateWater = false)
	{
		world = _world;
		seed = _seed;
		resourceInput = _resourceInput;
		playerPosition = _playerPosition;
		generateWater = _generateWater;

        LoadFile load = new();
		dynamic groundResources = load.LoadJson("groundResources.json");
		
        resource = groundResources[resourceInput];
		//Vector2I atlasCoords = new Vector2I((int)resource.atlasCoords[0], (int)resource.atlasCoords[1]);

        noise = new()
        {
            Seed = seed + (int)resource.addedSeed,
            Frequency = (float)resource.frequency
        };

		ChunkSpiral(generationWidth, generationHeight);
	}

	// Algorithm to generate chunks around the player in spiral
	private void ChunkSpiral(int X, int Y)
	{
		int x,y,dx,dy;
		x = y = dx = 0;
		dy = -1;
		int t = Math.Max(X,Y);
		int maxI = t * t;
		for(int i =0; i < maxI; i++)
		{
			if ((-X / 2 <= x) && (x <= X / 2) && (-Y / 2 <= y) && (y <= Y / 2))
			{
				/*Thread generateWorld = new(() => GenerateChunk(new(playerPosition.X + x * chunkSize, playerPosition.Y + y * chunkSize)));
				generateWorld.Start();*/
				GenerateChunk(new(playerPosition.X + x * chunkSize, playerPosition.Y + y * chunkSize));
			}
			if( (x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1-y)))
			{
				t = dx;
				dx = -dy;
				dy = t;
			}
			x += dx;
			y += dy;
		}
    }

	// Generates chunk based on location
	private void GenerateChunk(Vector2I chunkPosition)
	{
		float generationCap = (float)resource.generationCap;
		List<Vector2I> resourceCells = new();
		List<Vector2I> waterCells = new();

		switch (resourceInput)
		{
			case "Grass":
				if (grassChunks.Contains(chunkPosition))
				{
					return;
				}
				grassChunks.Add(chunkPosition);
				break;

			case "Water":
				if (waterChunks.Contains(chunkPosition))
				{
					return;
				}
				waterChunks.Add(chunkPosition);
				break;

			case "IronOre":
				if (ironChunks.Contains(chunkPosition))
				{
					return;
				}
				ironChunks.Add(chunkPosition);
				break;

			case "CopperOre":
				if (copperChunks.Contains(chunkPosition))
				{
					return;
				}
				copperChunks.Add(chunkPosition);
				break;	
		}

		for (int x = 0; x < chunkSize; x++)
		{
			for (int y = 0; y < chunkSize; y++)
			{
				float noiseValue = noise.GetNoise2D(chunkPosition.X + x, chunkPosition.Y + y);

				if (generationCap > noiseValue)
				{
					if (resourceInput == "Grass")
					{
						Vector2I atlasCoords = new Vector2I((int)resource.atlasCoords[0], (int)resource.atlasCoords[1]);
						world.SetCell(0, chunkPosition + new Vector2I(x, y), 0, atlasCoords);
					}
					else if ((string)world.GetCellTileData(0, chunkPosition + new Vector2I(x, y)).GetCustomData("resourceName") == "Grass")
					{
						resourceCells.Add(chunkPosition + new Vector2I(x, y));
					}
				}
				else if (generateWater)
				{
                    waterCells.Add(chunkPosition + new Vector2I(x, y));
				}
			}
		}

		Array<Vector2I> resourceCellsArray = new (resourceCells);
		Array<Vector2I> waterCellsArray = new (waterCells);
		
		world.SetCellsTerrainConnect(0, resourceCellsArray, 0, System.Array.IndexOf(groundTerrains, resourceInput));
		if (generateWater)
		{
            world.SetCellsTerrainConnect(0, waterCellsArray, 0, 1);
		}
	}

	public void ClearChunkList()
	{
		grassChunks = new();
		waterChunks = new();
		ironChunks = new();
		copperChunks = new();
	}
}
