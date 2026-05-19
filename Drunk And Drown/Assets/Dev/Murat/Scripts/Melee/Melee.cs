using UnityEngine;
public enum KindofMelee
{
    MorningStar,
    SwordFish
}
public class Melee : MonoBehaviour
{
    public KindofMelee Kind;
    public float cooldown = 1.0f;
    public float swingRate;
    public virtual void Swing()
    {

    }
}
