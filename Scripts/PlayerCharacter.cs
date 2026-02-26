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
    [ExportGroup("Stats")]
    [Export] public int nailDamage = 5;
    [Export] public int speed = 300;
    [Export] public int jumpStrength = 750;
    

    [Export] public int maxHealth = 5;
    [Export] public int currentHealth = 3;
    [Export] public int currentSoul = 3;

    Vector2 preVelocity = new Vector2(0, 0);
    Vector2 absolutePreVelocity;
    [ExportGroup("Forced Movement")]
    [Export] bool disableHorisontalControlls = false;
    [Export] bool disableJumping = false;
    [Export] public bool disableNail = false;
    [Export] Vector2 directionalPushVelocity = new Vector2(0,0);
    [Export] Vector2 nonDirectionalPushVelocity = new Vector2(0,0);
    [Export] Vector2 nonDirectionalLowerVelocityClamp = new Vector2(-1000000, -1000);
    [Export] Vector2 nonDirectionalHigherVelocityClamp = new Vector2(1000000, 1000);

    [ExportGroup("Tecnical")]
    Vector2 inputDirection = new Vector2(0, 0);
    [Export] public Direction directionToBe = Direction.Forward;

    [Export] bool altAttackAnim;

    bool cuttableJumping = false;
    Direction playerDirection = Direction.Forward;
    bool running = false;
    double coyote = .1;

    private AnimationTree animationTree;
    private Gui guiNode;

    public override void _Ready()
    {
        guiNode = GetNode<Gui>("CanvasLayer/gui");
        animationTree = GetNode<AnimationTree>("AnimationTree");
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

        if (!disableJumping && Input.IsActionJustPressed("jump") && coyote > 0) 
        {
            
            Jump(jumpStrength,true);
        }

        if (Input.IsActionJustReleased("jump") && cuttableJumping) preVelocity.Y = 10;

        preVelocity.X = (float)(Input.GetAxis("left","right") * speed * 50);
        if (disableHorisontalControlls) preVelocity.X = 0;


        

        if (Input.GetAxis("left", "right")!= 0)
        {
            directionToBe = (Direction)Math.Sign(Input.GetAxis("left", "right"));
            running = true;
        }else running = false;

        absolutePreVelocity = (
            preVelocity + 
            (directionalPushVelocity with {X = directionalPushVelocity.X* (int)playerDirection } + nonDirectionalPushVelocity) * 50)
            //.Clamp(directionalLowerVelocityClamp with { X = directionalLowerVelocityClamp.X * (int)playerDirection }, directionalHigherVelocityClamp with { X = directionalHigherVelocityClamp.X * (int)playerDirection })
            .Clamp(nonDirectionalLowerVelocityClamp, nonDirectionalHigherVelocityClamp);


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
        if (hitByNode.IsInGroup("knockback_applying")) animationTree.Set("parameters/knockback/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
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

    public void SetPaused(bool paused)
    {
        GetTree().Paused = paused;
    }

    public void Jump(int streangth, bool cuttable)
    {
        coyote = 0;
        preVelocity.Y = -streangth;
        cuttableJumping = cuttable;
    }

}
