using UnityEngine;
public enum KindofMelee
{
    MorningStar,
    SeaShellsBoxingGloves
}
public class Melee : Weapons
{
    public KindofMelee Kind;
    public float cooldown1 = 1.0f, cooldown2 = 1.0f;
    public float swingRate1, swingRate2;

    public override void ApplyRarityScaling(float multiplier)
    {
        if (multiplier > 0f)
        {
            swingRate1 /= multiplier;
            swingRate2 /= multiplier;
        }
    }

    public override float GetRate1() { return swingRate1; }
    public override float GetRate2() { return swingRate2; }
    public override string GetRate1Name() { return "Swing Delay"; }
    public override string GetRate2Name() { return "Secondary Delay"; }

    public virtual void Swing()
    {
        
    }
    public virtual void SecondDairy()
    {

    }
}
