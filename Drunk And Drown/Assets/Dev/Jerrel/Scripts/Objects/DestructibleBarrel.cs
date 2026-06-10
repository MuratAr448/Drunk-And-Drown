using System;
using UnityEngine;

public class DestructibleBarrel : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float currentHealth;

    [Header("Crash Settings")]
    [Tooltip("The minimum player velocity required to break this barrel on impact.")]
    [SerializeField] private float minVelocityToDestroy = 15f;

    [Header("Effects (Optional)")]
    [SerializeField] private GameObject destroyEffectPrefab;
    [SerializeField] private AudioEvent destroySound;

    [Header("Loot/Rewards (Optional)")]
    [SerializeField] private int coinsReward = 5;
    [SerializeField] private int scoreReward = 10;

    public float CurrentHealth => currentHealth;
    public float BaseHealth => maxHealth;
    public Action OnHealthChanged { get; set; }

    private bool isDestroyed = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDestroyed) return;

        // Proactively check player speed and distance to disable collider before physical impact occurs
        if (MainPlayer.Instance != null)
        {
            float distance = Vector3.Distance(transform.position, MainPlayer.Instance.transform.position);
            if (distance <= 2.2f)
            {
                if (MainPlayer.Instance.TryGetComponent<Movement>(out var movement))
                {
                    if (movement.GetVelocity() >= minVelocityToDestroy)
                    {
                        if (TryGetComponent<Collider>(out var col))
                        {
                            col.enabled = false;
                        }
                        Die();
                    }
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }

    public void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Disable collider immediately so the player passes right through without losing momentum
        if (TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }

        // Spawn optional particle/explosion prefab
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, transform.rotation);
        }

        // Play optional sound effect on a temporary GameObject so it doesn't get cut off when this object is destroyed
        if (destroySound != null)
        {
            GameObject tempAudioGo = new GameObject("TempAudio_" + destroySound.name);
            tempAudioGo.transform.position = transform.position;
            AudioSource tempSource = tempAudioGo.AddComponent<AudioSource>();
            
            // Set 3D spatial properties
            tempSource.spatialBlend = 1.0f;
            tempSource.minDistance = 1f;
            tempSource.maxDistance = 30f;
            
            destroySound.Play(tempSource);
            
            float duration = tempSource.clip != null ? tempSource.clip.length : 1f;
            Destroy(tempAudioGo, duration);
        }

        // Grant rewards if the game managers are active
        if (CoinSystem.Instance != null && coinsReward > 0)
        {
            CoinSystem.Instance.AddCoins(coinsReward, transform.position);
        }

        if (ScoreSystem.Instance != null && scoreReward > 0)
        {
            ScoreSystem.Instance.AddScore(scoreReward, transform.position);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckImpact(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckImpact(other.gameObject);
    }

    private void CheckImpact(GameObject otherObject)
    {
        if (isDestroyed) return;

        if (otherObject.CompareTag("Player"))
        {
            if (otherObject.TryGetComponent<Movement>(out var movement))
            {
                float playerSpeed = movement.GetVelocity();
                if (playerSpeed >= minVelocityToDestroy)
                {
                    Die();
                }
            }
        }
    }
}
