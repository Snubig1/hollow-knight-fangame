using Godot;
using System;

public partial class Bench : Node2D
{
    Area2D benchArea;
    Sprite2D sitPrompt;
    public override void _Ready()
    {
        benchArea = GetNode<Area2D>("BenchArea");
        sitPrompt = GetNode<Sprite2D>("SitPrompt");
        benchArea.BodyEntered += PlayerEntered;
        benchArea.BodyExited += PlayerExited;
    }

    private void PlayerEntered(Node2D body)
    {
        sitPrompt.Visible = true;
    }
    private void PlayerExited(Node2D body)
    {
        sitPrompt.Visible = false;
    }
}
