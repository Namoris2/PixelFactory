using Godot;
using System;

public partial class TutorialContainer : Panel
{
    public bool collapsed = false;
    Label label;
    TabContainer tutorial;

    public override void _Ready()
    {

        CollapsingMenu collapsingMenu = GetNode<CollapsingMenu>("PanelContainer/CollapsingMenu");
        collapsingMenu.CallDeferred("show");

        collapsingMenu.GetNode<Button>("Button").Pressed += ToggleCollapse;

        label = GetNode<Label>("PanelContainer/Label");
        tutorial = GetNode<TabContainer>("Tutorial");
    }

    public void ToggleCollapse()
    {

        if (collapsed)
        {
            Size = new Vector2(416, 516);
            label.Show();
            tutorial.Show();
            collapsed = false;
        }
        else
        {
            Size = new Vector2(47, 47);
            label.Hide();
            tutorial.Hide();
            collapsed = true;
        }

    }
}
