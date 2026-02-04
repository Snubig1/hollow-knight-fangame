using Godot;
using System;

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
    public static Vector2 CollapseVec2Array(Vector2[] array)
    {
        return new Vector2(
            array[0].X + array[1].X + array[2].X + array[3].X + array[4].X + array[5].X,
            array[0].Y + array[1].Y + array[2].Y + array[3].Y + array[4].Y + array[5].Y
            );
    }
}

interface IDamageable
{
    void Damage(int damage);
}