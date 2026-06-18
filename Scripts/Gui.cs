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

    public void SetMaxHealth(int pHealth)
    {
        emptyMasks.Size = new Vector2 (11*pHealth, emptyMasks.Size.Y);
    }
    public void SetHealth(int pHealth)
    {
        fullMasks.Size = new Vector2(11 * pHealth, fullMasks.Size.Y);
    }
    public void SetSoul(int pSoul)
    {
        if (pSoul <= 99)
        {
            soulMeter.Value = (pSoul/33f)*5 + 1;
            if (pSoul >= 45) soulMeter.TintOver = new Color(0xffffffff);
            else soulMeter.TintOver = new Color(0xffffff00);
        }
    }
}
