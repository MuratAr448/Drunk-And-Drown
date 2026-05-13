using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;
    [SerializeField] private GameObject particles;

    // Interface Properties
    public float CurrentHealth => _currentHealth;
    public float BaseHealth => _maxHealth;

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        GameObject hitParticles = Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(hitParticles, 3f);

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }


    }

    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}