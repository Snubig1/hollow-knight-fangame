using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtraFunctions;
using static Godot.OpenXRCompositionLayer;

/*
 for collision masks
   1 is the ground
   2 is enemy hurtboxes
   3 is enemy hitboxes
   4 is the player
   5 is a bench
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
    bool hasTakenDamageInFrame = false;

    private Bench lastBench;
    private string lastSavedScene;
    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationStateMachine;
    private TweenAnimationPlayer tweenPlayer;
    private Area2D hitBox;
    private Gui guiNode;

    public override void _Ready()
    {
        guiNode = GetNode<Gui>("CanvasLayer/gui");
        animationTree = GetNode<AnimationTree>("AnimationTree");
        animationStateMachine = animationTree.Get("parameters/AnimationNodeStateMachine/playback").As<AnimationNodeStateMachinePlayback>();
        tweenPlayer = new TweenAnimationPlayer(this);
        hitBox = GetNode<Area2D>("PlayerHitbox");
        preVelocity.Y = 0;
        guiNode.SetMaxHealth(maxHealth);
        guiNode.SetHealth(currentHealth);
        guiNode.SetSoul(currentSoul);
        hitBox.AreaShapeEntered += OnGetHit;
    }

    public override void _PhysicsProcess(double delta)
    {
        hasTakenDamageInFrame = false;
        inputDirection = Input.GetVector("left", "right", "up", "down").Normalized();

        if (IsOnFloor()) coyote = .1f;
        if (!IsOnFloor()) coyote -= delta;

        preVelocity.Y += GetGravity().Y*(float)delta;
        preVelocity.Y = Math.Clamp(preVelocity.Y,-1000, 1000);
        if (IsOnFloor() && preVelocity.Y > 0) preVelocity.Y = 0;
        if (IsOnCeiling()) preVelocity.Y = 10;

        if (cuttableJumping && preVelocity.Y >= 0) cuttableJumping = false;

        if (Input.IsActionJustPressed("up") && IsOnFloor())
        {
            interact();
        }

        if (!disableJumping && Input.IsActionJustPressed("jump") && coyote > 0) 
        {
            animationStateMachine.Travel("jump");
            //Jump(jumpStrength,true);
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
            .Clamp(nonDirectionalLowerVelocityClamp, nonDirectionalHigherVelocityClamp);


        Velocity = absolutePreVelocity with { X = absolutePreVelocity.X * (float)delta };

        MoveAndSlide();
    }

    public void Damage(int damage)
    {
        currentHealth = Math.Clamp(currentHealth - damage, 0, maxHealth);
        guiNode.SetHealth(currentHealth);
        if (currentHealth == 0)
        {
            Die();
        }
    }
    public void ChangeSoul(int soul)
    {
        currentSoul = Math.Clamp((int)(soul + currentSoul),0,30);
        guiNode.SetSoul(currentSoul);
    }

    public void OnGetHit(Rid area_rid, Area2D area, long body_shape_index, long local_shape_index)
    {
        if (hasTakenDamageInFrame) return;
        Node hitByNode = GetNodeOfAreaShape(area, (int)body_shape_index);
        if (hitByNode.IsInGroup("damageing")) 
        {
            Damage((int)hitByNode.GetMeta("damage", 1));
            hasTakenDamageInFrame = true;
        }
        if (!(currentHealth <= 0) && hitByNode.IsInGroup("knockback_applying")) animationTree.Set("parameters/knockback/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
    public void Die(bool dream = false)
    {
        animationStateMachine.Travel("death");
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
    public void Jump()
    {
        
        Jump(jumpStrength,true);
    }

    public void SitOnLastBench()
    {
        tweenPlayer.SitOnBench(lastBench);
    }

    public void RespawnAtLastBench()
    {
        GD.Print(lastSavedScene);

        GetTree().ChangeSceneToFile(lastSavedScene);
    }

    private bool interact()
    {
        Area2D interactable = hitBox.GetOverlappingAreas().ToList().Find(area => area.IsInGroup("interactable"));
        if (interactable == null) return false;
        if (interactable.GetParent().Name == "bench")
        {
            lastBench = interactable.GetParent<Bench>();
            lastSavedScene = GetTree().CurrentScene.SceneFilePath;
            animationTree.Set("parameters/AnimationNodeStateMachine/conditions/bench_sit", true);
        }
        return true;
    }
}
