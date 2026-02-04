using Godot;
using System;
using System.ComponentModel.Design;

public partial class Crawlid : BaseEnemy
{
    [Export] private int speed = 100;
    private int direction = 1;

    

    public override void _Ready()
    {
        //Transform = Transform with { X = new Vector2(direction * Transform.X.X, 0) };
        GetNode<Area2D>("HitArea").BodyEntered += DealDamage;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsOnFloor())
        {
            if (!GetNode<RayCast2D>("GroundDetectorRaycast2D").IsColliding()|| GetNode<RayCast2D>("WallDetectorRaycast2D").IsColliding())
            {
                direction *= -1;
                Transform = Transform with { X = new Vector2(direction* Math.Abs(Transform.X.X), 0) };
            }
            Velocity = new Vector2((float)(speed * direction), 0);
        } else Velocity = new Vector2(0,100);

        MoveAndSlide();

    }

    public void DealDamage(Node2D target)
    {
        PlayerCharacter target2 = (PlayerCharacter)target;
        if (target.IsInGroup("enemyDamageable"))
        {
            target2.Damage(1);
        }
    }
}
