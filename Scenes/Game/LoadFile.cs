using Godot;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

public partial class LoadFile : Node
{
	static Dictionary<string, dynamic> loadedJsons = new();
	
	// loads and parses Json file into object
    public static dynamic LoadJson(string path)
	{
		if (loadedJsons.ContainsKey(path)) { return loadedJsons[path]; }

		using var file = Godot.FileAccess.Open($"res://Jsons/{path}", Godot.FileAccess.ModeFlags.Read);
		string content = file.GetAsText();
		var loadedJson = JsonConvert.DeserializeObject<dynamic>(content);

		loadedJsons.Add(path, loadedJson);
		return loadedJson;
	}
}
