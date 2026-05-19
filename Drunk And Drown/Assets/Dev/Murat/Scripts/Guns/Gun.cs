using UnityEngine;
public enum KindofGun
{
    BunderBuss,
    ParrotGun
}
public class Gun : MonoBehaviour
{
    public KindofGun Kind;
    public float cooldown = 1.0f;
    public float shootRate;
    public virtual void Schoot()
    {

    }
    
}
