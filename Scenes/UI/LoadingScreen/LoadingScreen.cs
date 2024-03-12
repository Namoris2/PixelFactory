using Godot;
using Godot.Collections;
using System;

public partial class LoadingScreen : Control
{
	public string scenePath;
	private bool loading = false;
	ResourceLoader.ThreadLoadStatus loadStatus = 0;
	Godot.Collections.Array progress = new() {};

	ProgressBar loadingBar;
	Label loadingProgress;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		loadingBar = GetNode<ProgressBar>("LoadingBar");
		loadingProgress = GetNode<Label>("LoadingProgress");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (loading)
		{
			loadStatus = ResourceLoader.LoadThreadedGetStatus(scenePath, progress);

			loadingBar.Value = (float)progress[0];
			loadingProgress.Text = $"{(int)((float)progress[0] * 100)}%";

			if (loadStatus == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				PackedScene newScene = (PackedScene)ResourceLoader.LoadThreadedGet(scenePath);
				GetTree().ChangeSceneToPacked(newScene);
				QueueFree();
			}
		}
	}

	public void StartLoading()
	{
		if (scenePath == null) { GD.PrintErr("Scene Path not set"); return; }

		loading = true;
		ResourceLoader.LoadThreadedRequest(scenePath);
	}
}
