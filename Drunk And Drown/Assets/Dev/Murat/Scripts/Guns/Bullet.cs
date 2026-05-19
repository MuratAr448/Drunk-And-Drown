using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletType
{
    Normal,
    Explosive // Fixed spelling
}

public class Bullet : MonoBehaviour
{
    public BulletType type;
    public float damage = 1;
    public float timeTillDeath = 1;
    public float radius = 1;
    public float force = 1;

    private List<IDamageable> damagedObjects = new List<IDamageable>();

    void Start()
    {
        StartCoroutine(LimitTime());
    }

    private IEnumerator LimitTime()
    {
        yield return new WaitForSeconds(timeTillDeath);
        Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        // Don't hit the player who shot the bullet (optional check)
        if (other.CompareTag("Player") && type == BulletType.Normal) return;

        switch (type)
        {
            case BulletType.Normal:
                HandleDirectHit(other, damage);
                Destroy(gameObject);
                break;

            case BulletType.Explosive:
                Explode(other);
                break;
        }
    }

    private void HandleDirectHit(Collider other, float dmg)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null && !damagedObjects.Contains(damageable))
        {
            damageable.TakeDamage(dmg);
            damagedObjects.Add(damageable);

            // Apply impact force if a Rigidbody exists
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(transform.forward * force, ForceMode.Impulse);
            }
        }
    }

    private void Explode(Collider impactCollider)
    {
        // Check everything in the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider col in colliders)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null && !damagedObjects.Contains(damageable))
            {
                float finalDamage = (col == impactCollider) ? damage : damage * 0.5f;
                damageable.TakeDamage(finalDamage);
                damagedObjects.Add(damageable);
            }

            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Movement moveScript = rb.GetComponent<Movement>();
                if (moveScript != null)
                {
                    moveScript.Exposion();
                }

                rb.AddExplosionForce(force, transform.position, radius, force * 0.1f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}