using Godot;
using System;
using System.Collections.Generic;
using static ExtraFunctions;

/*
 for collision masks
   1 is the ground
   2 is enemy hurtboxes
   3 is enemy hitboxes
*/
public partial class PlayerCharacter : CharacterBody2D, IDamageable
{
    [Export] public int nailDamage = 5;
    [Export] public int speed = 300;
    [Export] public int jumpStrength = 500;
    

    [Export] public int maxHealth = 5;
    [Export] public int currentHealth = 3;
    [Export] public int currentSoul = 3;

    Vector2 preVelocity = new Vector2(0, 0);
    Vector2 absolutePreVelocity;
    [Export] Vector2 directionalPushVelocity = new Vector2();

    Vector2 inputDirection = new Vector2(0, 0);
    [Export] public Direction directionToBe = Direction.Forward;

    [Export] bool altAttackAnim;

    bool cuttableJumping = false;
    Direction playerDirection = Direction.Forward;
    bool running = false;
    double coyote = .1;

    private Gui guiNode;


    public override void _Ready()
    {
        guiNode = GetNode<Gui>("CanvasLayer/gui");
        Area2D hitBox = GetNode<Area2D>("PlayerHitbox");
        preVelocity.Y = 0;
        guiNode.SetMaxHealth(maxHealth);
        guiNode.SetHealth(currentHealth);
        guiNode.SetSoul(currentSoul);
        hitBox.AreaShapeEntered += OnGetHit;
    }

    public override void _PhysicsProcess(double delta)
    {
        inputDirection = Input.GetVector("left", "right", "up", "down").Normalized();

        if (IsOnFloor()) coyote = .1f;
        if (!IsOnFloor()) coyote -= delta;

        preVelocity.Y += GetGravity().Y*(float)delta;
        preVelocity.Y = Math.Clamp(preVelocity.Y,-1000, 1000);
        if (IsOnFloor()) preVelocity.Y = 0;
        if (IsOnCeiling()) preVelocity.Y = 10;

        if (cuttableJumping && preVelocity.Y >= 0) cuttableJumping = false;

        if (Input.IsActionJustPressed("jump") && coyote>0) 
        {
            Jump(jumpStrength,true);
        }

        if (Input.IsActionJustReleased("jump") && cuttableJumping) preVelocity.Y = 0;

        preVelocity.X = (float)(Input.GetAxis("left","right") * speed * 50);
        


        if (Input.GetAxis("left", "right")!= 0)
        {
            directionToBe = (Direction)Math.Sign(Input.GetAxis("left", "right"));
            running = true;
        }else running = false;

        absolutePreVelocity = (preVelocity + directionalPushVelocity with {X = directionalPushVelocity.X* (int)directionToBe } * 50);
        Velocity = absolutePreVelocity with { X = absolutePreVelocity.X * (float)delta };

        MoveAndSlide();
    }

    public void Damage(int damage)
    {
        currentHealth -= damage;
        guiNode.SetHealth(currentHealth);
    }
    public void ChangeSoul(int soul)
    {
        currentSoul = Math.Clamp((int)(soul + currentSoul),0,30);
        guiNode.SetSoul(currentSoul);
    }

    public void OnGetHit(Rid area_rid, Area2D area, long body_shape_index, long local_shape_index)
    {
        Node hitByNode = GetNodeOfAreaShape(area, (int)body_shape_index);
        if (hitByNode.IsInGroup("damageing")) Damage((int)hitByNode.GetMeta("damage", 1));
    }

    public void Turn(Direction direct)
    {
        Transform = Transform with { X = new Vector2((int)direct, 0f) };
        playerDirection = direct;
    }

    public void Turn()
    {
        Transform = Transform with { X = new Vector2((int)directionToBe, 0f) };
        playerDirection = directionToBe;
    }

    public enum Direction
    {
        Forward = 1,
        Backward = -1,
    }


    public void Jump(int streangth, bool cuttable)
    {
        coyote = 0;
        preVelocity.Y = -streangth;
        cuttableJumping = cuttable;
    }

}
