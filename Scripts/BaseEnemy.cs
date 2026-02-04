using Godot;
using System;

public partial class BaseEnemy : CharacterBody2D, IDamageable
{

    [Export] public int maxHealth;
    [Export] public int CurrentHealth;

    public void Death()
    {
        Modulate = new Color(0.5F,0.5F,0.5F);
        SetPhysicsProcess(false);
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void Damage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0) Death();
    }
}
