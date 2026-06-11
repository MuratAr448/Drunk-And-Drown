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
    public virtual void Swing()
    {
        
    }
    public virtual void SecondDairy()
    {

    }
}
