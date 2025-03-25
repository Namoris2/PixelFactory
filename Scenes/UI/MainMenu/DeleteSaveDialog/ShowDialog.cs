using Godot;
using System;

public partial class ShowDialog : Button
{
	DeleteSaveDialog deleteDialog;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		deleteDialog = (DeleteSaveDialog)GetTree().GetFirstNodeInGroup("DeleteSaveDialog");
		Pressed += ShowDeleteDialog;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void ShowDeleteDialog()
	{
		string saveName = GetNode<Label>("../SaveName").Text;
		string savePath = $"user://saves/{saveName}.save";

		deleteDialog.saveName = saveName;
		deleteDialog.savePath = savePath;
		deleteDialog.label.Text = $"Are you sure you want to delete this save?\nSave name:'{saveName}'";
		deleteDialog.index = GetParent().GetIndex();
		deleteDialog.Show();
	}
}
