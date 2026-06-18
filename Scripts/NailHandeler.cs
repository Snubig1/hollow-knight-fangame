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
    public void NailSwing(Vector2 pDirection)
    {
        if ((bool)animationTree.Get("parameters/MainAnimationStateMachine/conditions/Attacking") == false)
        {
            animationTree.Set("parameters/MainAnimationStateMachine/conditions/Attacking", true);
        }
    }

    public void OnNailHit(Rid pBodyRid, Node2D pBody, long pBodyShapeIndex, long pLocalShapeIndex)
    {
        Node hitNode = GetNodeOfBodyShape((CollisionObject2D)pBody, (int)pBodyShapeIndex);
        if (pLocalShapeIndex == 1 && hitNode.IsInGroup("pogoable")) player.Jump(700,false);
        if (hitNode.IsInGroup("soulfull")) player.ChangeSoul(11);

        if (hitNode.IsInGroup("damageable")) ((IDamageable)pBody).Damage(player.nailDamage);
        if (hitNode.IsInGroup("knockbackable")) ((IKnockbackable)pBody).TakeKnockback(player.knockbackStrength);

        if (pLocalShapeIndex == 0 && hitNode.IsInGroup("recoiling")) animationTree.Set("parameters/pushback/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
}