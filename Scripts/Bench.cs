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

    private void PlayerEntered(Node2D pBody)
    {
        sitPrompt.Visible = true;
    }
    private void PlayerExited(Node2D pBody)
    {
        sitPrompt.Visible = false;
    }

    public void sit(Node2D pSitter, Tween pTween)
    {
        GD.Print(pSitter, pTween);
        pTween.TweenProperty(pSitter, "position", this.GlobalPosition, 1.0f);
    }
}
