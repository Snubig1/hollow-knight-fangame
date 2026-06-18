using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtraFunctions;

/*
 for collision masks
   1 is the ground
   2 is enemy hurtboxes
   3 is enemy hitboxes
   4 is the player
   5 is a bench
*/
// you take damage if your HURTbox touches an enemy HITbox
 
public partial class PlayerCharacter : CharacterBody2D, IDamageable
{
    [ExportGroup("Stats")]
    [Export] public int knockbackStrength = 5;
    [Export] public int nailDamage = 5;
    [Export] public int speed = 300;
    [Export] public int jumpStrength = 750;
    [Export] public int maxHealth = 5;
    [Export] public int currentHealth = 3;
    [Export] public int currentSoul = 3;

    [Export] public double focusTime = 0.9;


    [ExportGroup("Forced Movement")]
    [Export] bool disableHorisontalControlls = false;
    [Export] bool disableJumping = false;
    [Export] public bool disableNail = false;
    [Export] Vector2 directionalPushVelocity = new Vector2(0,0);
    [Export] Vector2 nonDirectionalPushVelocity = new Vector2(0,0);
    [Export] Vector2 nonDirectionalLowerVelocityClamp = new Vector2(-1000000, -1000);
    [Export] Vector2 nonDirectionalHigherVelocityClamp = new Vector2(1000000, 1000);
    Vector2 preVelocity = new Vector2(0, 0);
    Vector2 absolutePreVelocity;

    [ExportGroup("Tecnical")]
    [Export] public Direction directionToBe = Direction.Forward;
    [Export] bool altAttackAnim;
    Vector2 inputDirection = new Vector2(0, 0);
    int hInputAxis = 0;
    bool cuttableJumping = false;
    Direction playerDirection = Direction.Forward;
    bool running = false;
    double coyote = .1;
    double focusStartUp = 0.25;
    double focusProgress = 0;
    int soulAfterFocus = 99;
    bool hasTakenDamageInFrame = false;
    private Bench lastBench;

    //memory
    private Vector2 lastSavePosition;
    private string lastSavedLevel;
    private AnimationTree animationTree;
    private AnimationNodeStateMachine animationStateMachine;
    private AnimationNodeStateMachinePlayback animationStateMachineController;
    private TweenAnimationPlayer tweenPlayer;
    private Area2D hitBox;
    private Gui guiNode;

    private States state = States.grounded;

    enum States
    {
        grounded,
        airborne,
        focusing,
        sitting,
        stunned
    }

    public override void _Ready()
    {
        //onLoad assignments
        guiNode = GetNode<Gui>("CanvasLayer/gui");
        animationTree = GetNode<AnimationTree>("AnimationTree");
        animationStateMachine = (AnimationNodeStateMachine)((AnimationNodeBlendTree)animationTree.TreeRoot).GetNode("MainAnimationStateMachine");
        animationStateMachineController = animationTree.Get("parameters/MainAnimationStateMachine/playback").As<AnimationNodeStateMachinePlayback>();
        tweenPlayer = new TweenAnimationPlayer(this);
        hitBox = GetNode<Area2D>("PlayerHurtbox");
        preVelocity.Y = 0;
        guiNode.SetMaxHealth(maxHealth);
        guiNode.SetHealth(currentHealth);
        guiNode.SetSoul(currentSoul);

        if (lastSavedLevel == null)
        {
            lastSavePosition = Position;
            lastSavedLevel = GetNode("../level").GetChild(0).SceneFilePath;
        }

        //Signal registration
        hitBox.AreaShapeEntered += OnGetHit;
    }


    public override void _PhysicsProcess(double delta)
    {
        hasTakenDamageInFrame = false;
        inputDirection = Input.GetVector("left", "right", "up", "down").Normalized();
        hInputAxis = (int)Math.Round(Input.GetAxis("left", "right"));
        //GD.Print(animationStateMachine);

        switch (state){
            //==============================
            case States.grounded:
                //groundedness and jumping
                coyote = .1f;
                if (preVelocity.Y > 0) preVelocity.Y = 0;

                if (Input.IsActionJustPressed("up")){
                    interact();
                }

                if (!disableJumping && coyote > 0 && Input.IsActionJustPressed("jump"))
                {
                    animationStateMachineController.Travel("jump");
                }

                //moving
                preVelocity.X = (hInputAxis * speed * 50);

                if (hInputAxis != 0)
                {
                    directionToBe = (Direction)Math.Sign(hInputAxis);
                    running = true;
                }
                else running = false;

                //state control
                if (Input.IsActionPressed("focus")) {
                    focusStartUp -= delta;
                    if (focusStartUp <= 0) {
                        soulAfterFocus = currentSoul - 33;
                        if (soulAfterFocus >= 0) {
                            state = States.focusing;
                            focusStartUp = 0.25;
                            focusProgress = 0;
                            preVelocity.X = 0;
                            running = false;
                        }
                        else focusStartUp = 0.25;
                    }
                }
                else focusStartUp = 0.25;

                if (!IsOnFloor()) state = States.airborne;
                break;

            //==============================
            case States.airborne:
                //falling
                coyote -= delta;
                preVelocity.Y += GetGravity().Y * (float)delta;
                preVelocity.Y = Math.Clamp(preVelocity.Y, -1000, 1000);
                if (IsOnCeiling()) preVelocity.Y = 10;
                if (cuttableJumping && preVelocity.Y >= 0) cuttableJumping = false;
                if (Input.IsActionJustReleased("jump") && cuttableJumping) preVelocity.Y = 10;


                //moving
                preVelocity.X = (hInputAxis * speed * 50);
                if (hInputAxis != 0)
                {
                    directionToBe = (Direction)Math.Sign(hInputAxis);
                }


                //state control
                if (IsOnFloor()) state = States.grounded;
                break;

            //==============================
            case States.focusing:

                focusProgress += delta;

                if (focusProgress >= focusTime) 
                {
                    focusProgress = 0;
                    ChangeSoul(-33);
                    soulAfterFocus = currentSoul - 33;
                    Heal(1);
                }


                //state control
                if (soulAfterFocus < 0 || !Input.IsActionPressed("focus"))
                {
                    state = States.grounded;

                }
                if (!IsOnFloor()) state = States.airborne;
                break;
        }
        

        absolutePreVelocity = (
            preVelocity + 
            (directionalPushVelocity with {X = directionalPushVelocity.X* (int)playerDirection } + nonDirectionalPushVelocity) * 50)
            .Clamp(nonDirectionalLowerVelocityClamp, nonDirectionalHigherVelocityClamp);


        Velocity = absolutePreVelocity with { X = absolutePreVelocity.X * (float)delta };

        MoveAndSlide();
    }

    public void Damage(int pDamage)
    {
        currentHealth = Math.Clamp(currentHealth - pDamage, 0, maxHealth);
        guiNode.SetHealth(currentHealth);
        if (currentHealth == 0)
        {
            Die();
        }
    }
    public void Heal(int pHealth)
    {
        currentHealth = Math.Clamp(currentHealth + pHealth, 0, maxHealth);
        guiNode.SetHealth(currentHealth);
    }

    public void ChangeSoul(int pSoul)
    {

        currentSoul = Math.Clamp((int)(pSoul + currentSoul),0,99);
        guiNode.SetSoul(currentSoul);
    }

    public void OnGetHit(Rid pAreaRid, Area2D pArea, long pBodyShapeIndex, long pLocalShapeIndex)
    {
        if (hasTakenDamageInFrame) return;
        Node hitByNode = GetNodeOfAreaShape(pArea, (int)pBodyShapeIndex);
        if (hitByNode.IsInGroup("damageing")) 
        {
            Damage((int)hitByNode.GetMeta("damage", 1));
            hasTakenDamageInFrame = true;
        }
        if (!(currentHealth <= 0) && hitByNode.IsInGroup("knockback_applying")) 
        {
            directionToBe = (Direction)Math.Sign((hitByNode.GetParent<Area2D>().GlobalPosition - GlobalPosition).X);
            Turn();
            animationTree.Set("parameters/knockback/request", (int)AnimationNodeOneShot.OneShotRequest.Fire); 
        }
    }
    public void Die(bool pDream = false)
    {
        animationStateMachineController.Travel("death");
    }

    public void Turn(Direction pDirection)
    {
        Transform = Transform with { X = new Vector2((int)pDirection, 0f) };
        playerDirection = pDirection;
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

    public void SetPaused(bool pPaused)
    {
        GetTree().Paused = pPaused;
    }

    public void Jump(int pStreangth, bool pCuttable)
    {
        coyote = 0;
        preVelocity.Y = -pStreangth;
        cuttableJumping = pCuttable;
    }
    public void Jump()
    {
        
        Jump(jumpStrength,true);
    }

    public void SitOnLastBench(float pTime)
    {
        tweenPlayer.SitOnBench(lastBench, pTime);
    }

    public void RespawnAtLastBench()
    {
        GetNode("../level").GetChild(0).QueueFree();
        Position = lastSavePosition;
        Heal(100);
        PackedScene levelToBe = GD.Load<PackedScene>(lastSavedLevel);

        GetNode("../level").AddChild(levelToBe.Instantiate());

    }

    private bool interact()
    {
        Area2D interactable = hitBox.GetOverlappingAreas().ToList().Find(area => area.IsInGroup("interactable"));

        if (interactable == null) return false;
        if (interactable.GetParent() is Bench)
        {
            lastBench = interactable.GetParent<Bench>();
            lastSavePosition = interactable.GetParent<Bench>().Position;
            lastSavedLevel = GetNode("../level").GetChild(0).SceneFilePath;
            animationTree.Set("parameters/MainAnimationStateMachine/conditions/bench_sit", true);
        }
        return true;
    }
}
