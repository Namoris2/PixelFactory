using Godot;
using System;

public partial class UIToggle : Node
{
    public bool toggle = false;

    bool ToggleUI()
    {
        toggle = !toggle;
        return toggle;
    }
}
