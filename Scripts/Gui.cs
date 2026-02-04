using Godot;
using System;

public partial class Gui : Control
{

    NinePatchRect emptyMasks;
    NinePatchRect fullMasks;
    TextureProgressBar soulMeter;

    public override void _Ready()
    {
        emptyMasks = GetNode<NinePatchRect>("EmptyMasks");
        fullMasks = GetNode<NinePatchRect>("FullMasks");
        soulMeter = GetNode<TextureProgressBar>("SoulMeter");
    }

    public void SetMaxHealth(int health)
    {
        emptyMasks.Size = new Vector2 (11*health, emptyMasks.Size.Y);
    }
    public void SetHealth(int health)
    {
        fullMasks.Size = new Vector2(11 * health, fullMasks.Size.Y);
    }
    public void SetSoul(int soul)
    {
        if (soul <= 30)
        {
            soulMeter.Value = soul * 0.5 + 1;
            if (soul >= 16) soulMeter.TintOver = new Color(0xffffffff);
            else soulMeter.TintOver = new Color(0xffffff00);
        }
    }
}
