using UnityEngine;
public enum KindofGun
{
    BunderBuss,
    ParrotGun,
    SquidRayGun
}
public class Gun : Weapons
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

    protected float lastDisableTime;

    protected virtual void OnEnable()
    {
        if (lastDisableTime > 0f)
        {
            float timePassed = Time.time - lastDisableTime;
            cooldown1 = Mathf.Min(cooldown1 + timePassed, shootRate1);
            cooldown2 = Mathf.Min(cooldown2 + timePassed, shootRate2);
        }
    }

    protected virtual void OnDisable()
    {
        lastDisableTime = Time.time;
    }
}
