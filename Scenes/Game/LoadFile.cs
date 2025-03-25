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

		FileAccess loadedJson = Godot.FileAccess.Open($"res://Jsons/{path}", Godot.FileAccess.ModeFlags.Read);
		string jsonText = loadedJson.GetAsText();
		dynamic convertedJson = JsonConvert.DeserializeObject<dynamic>(jsonText);

		loadedJsons.Add(path, convertedJson);
		return convertedJson;
	}
}
