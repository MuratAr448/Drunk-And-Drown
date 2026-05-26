using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarModifier;
    private IDamageable damageableTarget;

    private void Start()
    {
        damageableTarget = GetComponentInParent<IDamageable>();

        if (damageableTarget != null)
        {
            damageableTarget.OnHealthChanged += UpdateHealthDisplay;
            UpdateHealthDisplay();
        }
    }

    private void UpdateHealthDisplay()
    {
        if (damageableTarget == null || healthBarModifier == null) return;

        float healthPercent = damageableTarget.CurrentHealth / damageableTarget.BaseHealth;
        healthBarModifier.fillAmount = healthPercent;
    }

    private void OnDestroy()
    {
        if (damageableTarget != null)
        {
            damageableTarget.OnHealthChanged -= UpdateHealthDisplay;
        }
    }
}