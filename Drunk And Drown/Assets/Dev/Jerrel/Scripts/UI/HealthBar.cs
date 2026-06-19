using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarModifier;
    [SerializeField] private float lerpSpeed = 5f;
    
    private IDamageable damageableTarget;
    private float targetFillAmount = 1f;

    private void Start()
    {
        damageableTarget = GetComponentInParent<IDamageable>();

        if (damageableTarget != null)
        {
            damageableTarget.OnHealthChanged += UpdateHealthDisplay;
            
            // Set initial health instantly at start
            float healthPercent = damageableTarget.BaseHealth > 0 ? (damageableTarget.CurrentHealth / damageableTarget.BaseHealth) : 1f;
            targetFillAmount = Mathf.Clamp01(healthPercent);
            if (healthBarModifier != null)
            {
                healthBarModifier.fillAmount = targetFillAmount;
            }
        }
    }

    private void Update()
    {
        if (healthBarModifier == null) return;

        if (Mathf.Abs(healthBarModifier.fillAmount - targetFillAmount) > 0.001f)
        {
            healthBarModifier.fillAmount = Mathf.Lerp(healthBarModifier.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
        }
        else
        {
            healthBarModifier.fillAmount = targetFillAmount;
        }
    }

    private void UpdateHealthDisplay()
    {
        if (damageableTarget == null) return;

        float healthPercent = damageableTarget.BaseHealth > 0 ? (damageableTarget.CurrentHealth / damageableTarget.BaseHealth) : 1f;
        targetFillAmount = Mathf.Clamp01(healthPercent);
    }

    private void OnDestroy()
    {
        if (damageableTarget != null)
        {
            damageableTarget.OnHealthChanged -= UpdateHealthDisplay;
        }
    }
}