using Godot;
using System;

using static ExtraFunctions;


public partial class NailHandeler : Node2D
{

    AnimationTree animationTree;
    PlayerCharacter player;

    Node[] hitBodies;

    public override void _Ready()
    {
        animationTree = GetNode<AnimationTree>("../AnimationTree");
        Area2D nailArea = GetNode<Area2D>("nail_area");
        nailArea.BodyShapeEntered += OnNailHit;
        player = GetParent<PlayerCharacter>();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!player.disableNail && Input.IsActionJustPressed("nail"))
        {
            NailSwing(Input.GetVector("left", "right", "up", "down").Normalized());
        }
    }
    public void NailSwing(Vector2 direction)
    {
        if ((bool)animationTree.Get("parameters/AnimationNodeStateMachine/conditions/Attacking") == false)
        {
            animationTree.Set("parameters/AnimationNodeStateMachine/conditions/Attacking", true);
        }
    }

    public void OnNailHit(Rid body_rid, Node2D body, long body_shape_index, long local_shape_index)
    {
        Node hitNode = GetNodeOfBodyShape((CollisionObject2D)body, (int)body_shape_index);
        if (local_shape_index == 1 && hitNode.IsInGroup("pogoable")) player.Jump(700,false);
        if (hitNode.IsInGroup("soulfull")) player.ChangeSoul(2);

        if (hitNode.IsInGroup("damageable")) ((IDamageable)body).Damage(player.nailDamage);

        if (local_shape_index == 0 && hitNode.IsInGroup("recoiling")) animationTree.Set("parameters/pushback/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
}