using Godot;
using System;
using Newtonsoft.Json;

public partial class LoadFile : Node
{
	// loads and parses Json file into object
    public dynamic LoadJson(string path)
	{
		using var file = Godot.FileAccess.Open($"res://jsons/{path}", Godot.FileAccess.ModeFlags.Read);
		string content = file.GetAsText();
		var loadedJson = JsonConvert.DeserializeObject<dynamic>(content);
		return loadedJson;
	}
}
