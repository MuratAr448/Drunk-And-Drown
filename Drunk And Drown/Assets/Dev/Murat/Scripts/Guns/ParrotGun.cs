using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParrotGun : MonoBehaviour
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
    public float cooldown = 1.0f;
    public float shootRate;
    [SerializeField] private Animation _currentAnimation;
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
    }

    public void Shooting()
    {
        if (shootRate <= cooldown)
        {
            StartCoroutine(Shoot());
        }
    }
    private IEnumerator Shoot()
    {

        _currentAnimation.Play();
        _currentAnimation[_currentAnimation.clip.name].speed = 4f;
        yield return new WaitForSeconds(0.5f);
        if (activeBullet == null)
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
        yield return new WaitForSeconds(Time.deltaTime);
        cooldown = 0f;

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
        Vector3 targetPoint;
        int mask = ~(1 << mainPlayer.gameObject.layer);

        if (Physics.Raycast(origin.transform.position, origin.transform.forward, out RaycastHit hit, 1000f, mask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = origin.transform.position + origin.transform.forward * 100f;
        }

        Vector3 fireDirection = (targetPoint - activeBullet.transform.position).normalized;
        bulletRb.transform.LookAt(targetPoint);

        bulletRb.AddForce(fireDirection * 500f * bulletSpeed, ForceMode.Force);

        Vector3 knockbackDirection = -fireDirection;
        player.ApplyKnockback(knockbackDirection * force * 2.5f);

        activeBullet = null;
    }

    void Update()
    {
        if (shootRate >= cooldown)
        {
            cooldown += Time.deltaTime;
        }


    }
}