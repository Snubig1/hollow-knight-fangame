using Godot;
using System;
using System.ComponentModel.Design;

public partial class MessyMossCharger : BaseEnemy, IKnockbackable
{
    [Export] private int floorHeight;
    [Export] private Area2D vision;
    [Export] private States state = States.passive;
    private RayCast2D rayCast;
    private AnimationPlayer animationPlayer;
    private PlayerCharacter player;
    private double attackCooldown = 3;
    private Vector2 attackPosition;


    enum States
    {
        passive,
        inAnimation,
        scanning,
        dashing,
        jumping,
        stunned
    }

    public override void _Ready()
    {
        Position = new Vector2(Position.X, floorHeight);
        rayCast = GetNode<RayCast2D>("raycast");
        animationPlayer = GetNode<AnimationPlayer>("messy_animation_player");
        vision.CollisionMask = 8;
        vision.CollisionLayer = 0;
        vision.BodyEntered += SeePlayer;
    }

    public override void _PhysicsProcess(double delta)
    {
        switch (state)
        {
            //==============================
            case States.passive:
                Velocity = new Vector2(0, 0);


                if (player != null)
                {
                    attackCooldown = 0;
                    animationPlayer.Play("roar");
                }
                break;
            //==============================
            case States.scanning:

                attackCooldown -= delta;
                if (attackCooldown < 0)
                {
                    rayCast.GlobalPosition = player.GlobalPosition;
                    rayCast.TargetPosition = new Vector2(200, 0);
                    rayCast.ForceRaycastUpdate();
                    if (rayCast.IsColliding())
                    {
                        if (rayCast.GetCollisionPoint().DistanceTo(player.GlobalPosition) < 425) 
                            attackPosition = player.GlobalPosition with { X = player.GlobalPosition.X + 275 };
                        else attackPosition = rayCast.GetCollisionPoint() with {X = rayCast.GetCollisionPoint().X -200};
                    }
                    else attackPosition = player.GlobalPosition with { X = player.GlobalPosition.X + 600 };
                    if (new Random().Next(0, 2) == 1){
                        rayCast.TargetPosition = new Vector2(-200, 0);
                        rayCast.ForceRaycastUpdate();
                        if (rayCast.IsColliding())
                        {
                            if (rayCast.GetCollisionPoint().DistanceTo(player.GlobalPosition) < 425) 
                                attackPosition = player.GlobalPosition with { X = player.GlobalPosition.X - 275 };
                            else attackPosition = rayCast.GetCollisionPoint() with { X = rayCast.GetCollisionPoint().X + 200 };
                        }
                        else attackPosition = player.GlobalPosition with { X = player.GlobalPosition.X - 600 };
                    }
                    Position = attackPosition;
                    animationPlayer.Play("start_dash");
                }

                break;
            //==============================aaa
            case States.dashing:


                break;
            //==============================
            case States.jumping:


                break;
            //==============================
            case States.stunned:


                break;
        }
        MoveAndSlide();
    }

    public void TakeKnockback(int pStrength)
    {

    }


    public void SeePlayer(Node2D body)
    {
        player = (PlayerCharacter)body;
    }
}
