using Godot;
using System;


/*
 for collision masks
   1 is the ground
   2 is enemy hurtboxes
   3 is enemy hitboxes
   4 is the player
   5 is a bench
*/
public static class ExtraFunctions
{
    public static Node GetNodeOfBodyShape(CollisionObject2D body, int body_shape_index)
    {
        return (Node)body.ShapeOwnerGetOwner((body).ShapeFindOwner((int)body_shape_index));
    }
    public static Node GetNodeOfAreaShape(CollisionObject2D body, int body_shape_index)
    {
        return (Node)body.ShapeOwnerGetOwner((body).ShapeFindOwner((int)body_shape_index));
    }
}
public class TweenAnimationPlayer(Node pOwner)
{
    private Tween _tween;

    private void ClearAnimation()
    {
        if (_tween != null)
            _tween.Kill();
        _tween = pOwner.CreateTween();
    }
    public void SitOnBench(Bench pBench, float pTime)
    {
        ClearAnimation();
        _tween.SetTrans(Tween.TransitionType.Linear);
        _tween.TweenProperty(pOwner, "position", pBench.GlobalPosition, pTime);
        
    }
}


interface IDamageable
{
    void Damage(int pDamage);
}
interface IKnockbackable
{
    void TakeKnockback(int pStrength);
}

