using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsHandler : Node
{	public static readonly string[] keyBinds = new string[] { "MoveUp", "MoveDown", "MoveLeft", "MoveRight", "Sprint", "Interact", "Rotate", "ToggleBuildMode", "ToggleDismantleMode", "ToggleInventory", "ToggleResearchMenu"};
	public static readonly Dictionary<string, string> actions = new()
	{
		{ "MoveUp", "Move Up" },
		{ "MoveDown", "Move Down" },
		{ "MoveLeft", "Move Left" },
		{ "MoveRight", "Move Right" },
		{ "Sprint", "Sprint" },
		{ "Interact", "Interact" },
		{ "Rotate", "Rotate Building" },
		{ "ToggleBuildMode", "Open Build Menu" },
		{ "ToggleDismantleMode", "Dismantle Mode" },
		{ "ToggleInventory", "Open Inventory" },
        { "ToggleResearchMenu", "Open Research Menu" },

        { "Back", "Back" },
        { "Use", "Use" },
        { "SplitItems", "Split Stack" }
	};
    public static Dictionary<string, InputEvent> defaultBinds = new();
    const string configPath = "user://settings.ini";
    ConfigFile configFile = new();
    public override void _Ready()
	{
        if (!FileAccess.FileExists(configPath))
        {
            CreateConfigFile();
        }
        else
        {
            LoadConfigFile();
        }
	}

    private void CreateConfigFile()
    {
        // Set config values
        foreach (string keyBind in keyBinds)
        {
            InputEvent defaultBind = InputMap.ActionGetEvents(keyBind)[0];
            defaultBinds.Add(keyBind, defaultBind);

            configFile.SetValue("KeyboardMouseBinds", keyBind, defaultBind.AsText().TrimSuffix(" (Physical)"));
        }

        configFile.Save(configPath);
        GD.Print("Created config file");
    }

    private void LoadConfigFile()
    {
        Error err = configFile.Load(configPath);
        if (err != Error.Ok) { return; }

        string[] keyLoadedBinds = configFile.GetSectionKeys("KeyboardMouseBinds");

        foreach (string keyBind in keyLoadedBinds)
        {
            InputEvent defaultBind = InputMap.ActionGetEvents(keyBind)[0];
            defaultBinds.Add(keyBind, defaultBind);

            Godot.Collections.Array<InputEvent> inputs = InputMap.ActionGetEvents(keyBind);
            string keyEvent = (string)configFile.GetValue("KeyboardMouseBinds", keyBind);

            if (keyEvent.Contains("Mouse"))
            {
                InputEventMouseButton loadedButton = new();
                string mouseButton = keyEvent.Replace("Mouse", "");
                loadedButton.ButtonIndex = (MouseButton)Enum.Parse(typeof(MouseButton), mouseButton);
                inputs[0] = loadedButton;
            }
            else
            {
                InputEventKey loadedKey = new();
                loadedKey.Keycode = OS.FindKeycodeFromString(keyEvent);
                inputs[0] = loadedKey;
            }
            
            InputMap.ActionEraseEvents(keyBind);

            for (int i = 0; i < inputs.Count; i++)
            {
                InputMap.ActionAddEvent(keyBind, inputs[i]);
            }
        }
    }

    public void SaveConfigFile(string section, string action, dynamic value)
    {
        switch (section)
        {
            case "KeyboardMouseBinds":
                configFile.SetValue(section, action, value);
                configFile.Save(configPath);

                GD.Print($"Saved action '{action}' as '{value}' in section '{section}'");
                break;
            
            default:
                GD.Print($"Unknown config section '{section}'");
                break;
        }
    }
}
