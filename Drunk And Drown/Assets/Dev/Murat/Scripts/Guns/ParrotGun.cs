using UnityEngine;
using UnityEngine.InputSystem;

public class ParrotGun : Gun
{
    [SerializeField] private GameObject parrotPrefab;
    [SerializeField] private Transform transformPivot;
    [SerializeField] private float bulletSpeed = 1.0f;
    [SerializeField] private float damage = 3f;
    [SerializeField] private float radius = 4f;
    [SerializeField] private float force = 5f;

    [Header("Sounds")]
    [SerializeField] private AudioEvent shootSound;
    private AudioSource audioSource;

    private Movement player;
    private GameObject activeBullet;
    private float ammoLife = 3f;
    private MainPlayer mainPlayer;

    public override float GetDamage() { return damage; }
    public override void ApplyRarityScaling(float multiplier)
    {
        base.ApplyRarityScaling(multiplier);
        damage *= multiplier;
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.0f; // 2D sound for player weapon
    }

    void Start()
    {
        player = FindFirstObjectByType<Movement>();
        if (player != null)
        {
            mainPlayer = player.GetComponent<MainPlayer>();
        }
        Kind = KindofGun.ParrotGun;
    }

    public override void Shoot()
    {
        base.Shoot();

        if (shootRate1 <= cooldown1 && activeBullet != null)
        {
            cooldown1 = 0f;

            if (shootSound != null && audioSource != null)
            {
                shootSound.Play(audioSource);
            }

            Rigidbody bulletRb = activeBullet.GetComponent<Rigidbody>();
            Bullet ammunition = activeBullet.GetComponent<Bullet>();
            SphereCollider bulletCollider = activeBullet.GetComponent<SphereCollider>();

            activeBullet.transform.parent = null;
            bulletRb.isKinematic = false;
            if (bulletCollider != null) bulletCollider.enabled = true;

            ammunition.damage = damage;
            ammunition.timeTillDeath = ammoLife;
            ammunition.type = BulletType.Explosive;
            ammunition.radius = radius;
            ammunition.force = force;
            ammunition.Lanched();

            GameObject origin = mainPlayer.rayOrigin;
            Vector3 fireDirection = origin.transform.forward;

            if (Physics.Raycast(origin.transform.position, fireDirection, out RaycastHit hit))
            {
                fireDirection = (hit.point - activeBullet.transform.position).normalized;
                bulletRb.transform.LookAt(hit.point);
            }

            bulletRb.AddForce(fireDirection * 100f * bulletSpeed, ForceMode.Force);

            Vector3 knockbackDirection = -fireDirection;
            player.ApplyKnockback(knockbackDirection * force * 2.5f);

            activeBullet = null;
        }
    }

    public override void Secondary()
    {
        base.Secondary();
        if (shootRate2 <= cooldown2 && activeBullet != null)
        {
            cooldown2 = 0f;
        }
    }

    void Update()
    {
        if (shootRate1 >= cooldown1)
        {
            cooldown1 += Time.deltaTime;
        }
        if (shootRate2 >= cooldown2)
        {
            cooldown2 += Time.deltaTime;
        }

        if (activeBullet == null && shootRate1 <= cooldown1 + Time.deltaTime * 3f)
        {
            activeBullet = Instantiate(parrotPrefab, transformPivot);

            if (activeBullet.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }
            if (activeBullet.TryGetComponent<SphereCollider>(out SphereCollider sc))
            {
                sc.enabled = false;
            }
        }
    }
}