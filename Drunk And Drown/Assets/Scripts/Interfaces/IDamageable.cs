using UnityEngine;

public interface IDamageable
{
    float CurrentHealth { get; }
    float BaseHealth { get; }

    void TakeDamage(float damage);
    void ResetHealth();
    void Die();
}