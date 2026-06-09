using UnityEngine;
public enum KindofGun
{
    BunderBuss,
    ParrotGun,
    GrappleGun
}
public class Gun : MonoBehaviour
{
    public KindofGun Kind;
    public float cooldown1 = 1.0f, cooldown2 = 1.0f;
    public float shootRate1, shootRate2;
    public virtual void Shoot()
    {

    }
    public virtual void Secondary()
    {

    }
}
