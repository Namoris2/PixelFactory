using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;

public partial class LoadingScreen : Control
{
	[Signal] public delegate Error StartLoadingEventEventHandler(string path, string typeHint, bool useSubThreads, ResourceLoader.CacheMode cacheMode);
	public string scenePath;
	string savePath;
	private bool loading = false;
	public bool loadingSave = false;
	private bool requestToLoad = false;
	ResourceLoader.ThreadLoadStatus loadStatus = 0;
	Godot.Collections.Array progress = new() {};

	ProgressBar loadingBar;
	Label loadingProgress;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Loading started");
		loadingBar = GetNode<ProgressBar>("LoadingBar");
		loadingProgress = GetNode<Label>("LoadingProgress");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (requestToLoad)
		{
			ResourceLoader.LoadThreadedRequest(scenePath);
			requestToLoad = false;
			loading = true;
		}


		if (loading)
		{
			loadStatus = ResourceLoader.LoadThreadedGetStatus(scenePath, progress);

			loadingBar.Value = (float)progress[0];
			loadingProgress.Text = $"{(int)((float)progress[0] * 100)}%";

			if (loadStatus == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				//GD.Print("Game Loaded");
				PackedScene newScene = (PackedScene)ResourceLoader.LoadThreadedGet(scenePath);
				Node sceneInstantiated = newScene.Instantiate();
				GetTree().Root.AddChild(sceneInstantiated);
				
				if (loadingSave)
				{
					FileAccess saveFile = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
					string savedGame = saveFile.GetAsText();
					saveFile.Close();
					Array<Node> nodes = GetTree().GetNodesInGroup("CanSave");

					if (!Godot.FileAccess.FileExists(savePath)) { return; }
				
					//string[] savedGameList = savedGame.Split("\n");

					//int i = 0;
					foreach (dynamic node in nodes)
					{
						dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(savedGame);
						node.Load(data);
						//i++;
					}
					GD.Print("Save Loaded");
				}
				QueueFree();
			}
		}
	}

	public void StartLoading()
	{		
		if (scenePath == null) { GD.PrintErr("Scene Path not set"); return; }

		savePath = GetNode<main>("/root/GameInfo").savePath;
		/*StartLoadingEvent += ResourceLoader.LoadThreadedRequest;
		EmitSignal(SignalName.StartLoadingEvent, scenePath, "", false, 0);
		loading = true;*/
		requestToLoad = true;


		/*PackedScene newScene = GD.Load<PackedScene>("res://Scenes/main.tscn");
		Node sceneInstantiated = newScene.Instantiate();
		GetTree().Root.AddChild(sceneInstantiated);
		
		if (loadingSave)
		{
			FileAccess saveFile = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
			string savedGame = saveFile.GetAsText();
			saveFile.Close();
			Array<Node> nodes = GetTree().GetNodesInGroup("CanSave");

			if (!Godot.FileAccess.FileExists(savePath)) { return; }
		
			//string[] savedGameList = savedGame.Split("\n");

			//int i = 0;
			foreach (dynamic node in nodes)
			{
				dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(savedGame);
				node.Load(data);
				//i++;
			}
			//GD.Print("Save Loaded");
		}
		QueueFree();*/
	}
}
