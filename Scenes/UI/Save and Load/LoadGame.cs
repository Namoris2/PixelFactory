using Godot;
using Godot.Collections;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;

public partial class LoadGame : Button
{
	[Export] bool loadSave = true;
	string saveName;
    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += CheckAllParameters;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void CheckAllParameters()
	{
		GD.Print("Load pressed");
		if (Name != "CreateGame")
		{
			saveName = GetNode<Label>("../SaveName").Text;
			ChangeToLoadingScreen();
			return;
		}
		saveName = GetNode<LineEdit>("../SaveNameInput").Text;
		string seed = GetNode<LineEdit>("../SeedInput").Text;
		Label errorMessage = GetNode<Label>("../ErrorMessage");
		
		if (saveName != "")
		{
			if (!GetNode<MainMenu>("/root/MainMenu").saves.Contains(saveName + ".save"))
			{
				string convertedSeed = "";
				if (seed != "")
				{
					foreach (char letter in seed)
					{
						if (char.IsNumber(letter))
						{
							convertedSeed += letter;
						}
						else
						{
							convertedSeed += (letter % 32).ToString();
						}

						if (long.Parse(convertedSeed) > int.MaxValue)
						{
							convertedSeed = int.MaxValue.ToString();
							break;
						}
					}
					seed = convertedSeed;
				}
				else
				{
					Random rnd = new();
					seed = rnd.Next().ToString();
				}

				GetNode<main>("/root/GameInfo").seed = int.Parse(seed);
				ChangeToLoadingScreen();
			}
			else
			{
				errorMessage.Text = "Game with this name already exists!";
			}
		}
		else
		{
			errorMessage.Text = "Game name cannot be empty!";
		}
	}

	private void ChangeToLoadingScreen()
	{
		LoadingScreen loadingScreen = (LoadingScreen)GD.Load<PackedScene>("res://Scenes/UI/LoadingScreen/LoadingScreen.tscn").Instantiate();
		
		GetNode<main>("/root/GameInfo").savePath = "user://saves/" + saveName + ".save";
		loadingScreen.loadingSave = loadSave;
		loadingScreen.scenePath = "res://Scenes/main.tscn";
		GetTree().Root.AddChild(loadingScreen);

		GetNode<Node>("/root/MainMenu").QueueFree();
		loadingScreen.StartLoading();

		//GD.Print("scene changed");
		return;
	}
}
