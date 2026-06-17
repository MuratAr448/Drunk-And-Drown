using UnityEngine;

public class Weapons : MonoBehaviour
{
    public virtual void ApplyRarityScaling(float multiplier) {}
    public virtual float GetDamage() { return 0f; }
    public virtual float GetRate1() { return 0f; }
    public virtual float GetRate2() { return 0f; }
    public virtual string GetRate1Name() { return "Cooldown 1"; }
    public virtual string GetRate2Name() { return "Cooldown 2"; }
}
