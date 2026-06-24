using UnityEngine;

public class Weapons : MonoBehaviour
{
    [Header("Weapon Sounds")]
    [SerializeField] protected AudioEvent shootSound;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.0f; // 2D sound for player weapon
    }

    public virtual void ApplyRarityScaling(float multiplier) {}
    public virtual float GetDamage() { return 0f; }
    public virtual float GetRate1() { return 0f; }
    public virtual float GetRate2() { return 0f; }
    public virtual string GetRate1Name() { return "Cooldown 1"; }
    public virtual string GetRate2Name() { return "Cooldown 2"; }
}
