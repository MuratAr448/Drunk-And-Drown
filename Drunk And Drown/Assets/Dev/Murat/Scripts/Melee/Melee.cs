using UnityEngine;
public enum KindofMelee
{
    MorningStar,
    SwordFish
}
public class Melee : MonoBehaviour
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
