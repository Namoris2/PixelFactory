using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsHandler : Node
{	readonly string[] keyBinds = new string[] { "MoveUp", "MoveDown", "MoveLeft", "MoveRight","Sprint","Interact","Rotate", "ToggleBuildMode", "ToggleDismantleMode", "ToggleInventory"};
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
            configFile.SetValue("KeyboardMouseBinds", keyBind, InputMap.ActionGetEvents(keyBind)[0].AsText().TrimSuffix(" (Physical)"));
        }

        configFile.Save(configPath);
        GD.Print("Created config file");
    }

    private void LoadConfigFile()
    {
        Error err = configFile.Load(configPath);
        if (err != Error.Ok) { return; }

        string[] keyBinds = configFile.GetSectionKeys("KeyboardMouseBinds");

        foreach (string bind in keyBinds)
        {
            InputEventKey loadedBind = new();
            Godot.Collections.Array<InputEvent> inputs = InputMap.ActionGetEvents(bind);
            string keyEvent = (string)configFile.GetValue("KeyboardMouseBinds", bind);

            loadedBind.Keycode = OS.FindKeycodeFromString(keyEvent);
            inputs[0] = loadedBind;
            
            InputMap.ActionEraseEvents(bind);

            for (int i = 0; i < inputs.Count; i++)
            {
                InputMap.ActionAddEvent(bind, inputs[i]);
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
