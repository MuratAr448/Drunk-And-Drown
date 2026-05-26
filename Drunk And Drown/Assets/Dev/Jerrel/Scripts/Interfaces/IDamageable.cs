using System;
using UnityEngine;

public interface IDamageable
{
    float CurrentHealth { get; }
    float BaseHealth { get; }
    Action OnHealthChanged { get; set; }
    void TakeDamage(float damage);
    void ResetHealth();
    void Die();
}