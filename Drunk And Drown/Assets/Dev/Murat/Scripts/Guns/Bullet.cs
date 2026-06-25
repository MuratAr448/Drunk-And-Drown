using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletType
{
    Normal,
    Explosive, // Fixed spelling
    Ray
}

public class Bullet : MonoBehaviour
{
    public BulletType type;
    public float damage = 1;
    public float timeTillDeath = 1;
    public float radius = 1;
    public float force = 1;
    private List<IDamageable> damagedObjects = new List<IDamageable>();

    private Vector3 previousPosition;
    private bool hasCollided = false;

    private void Start()
    {
        previousPosition = transform.position;
    }

    public void Lanched()
    {
        StartCoroutine(LimitTime());
    }
    private IEnumerator LimitTime()
    {
        yield return new WaitForSeconds(timeTillDeath);
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (hasCollided) return;

        Vector3 currentPos = transform.position;
        Vector3 displacement = currentPos - previousPosition;
        float distance = displacement.magnitude;

        if (distance > 0.001f)
        {
            Vector3 direction = displacement.normalized;
            int mask = ~0;
            RaycastHit hit;
            if (Physics.Raycast(previousPosition, direction, out hit, distance, mask, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.CompareTag("Player") && !hit.collider.isTrigger)
                {
                    transform.position = hit.point;
                    HandleCollision(hit.collider);
                    return;
                }
            }
        }
        previousPosition = currentPos;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;

        // Ignore trigger colliders (like spawn zones, checkpoints, collectables, etc.)
        if (other.isTrigger) return;

        // Don't hit the player who shot the bullet (optional check)
        if (other.CompareTag("Player")) return;

        HandleCollision(other);
    }

    private void HandleCollision(Collider other)
    {
        if (hasCollided) return;
        hasCollided = true;

        switch (type)
        {
            case BulletType.Normal:
                HandleDirectHit(other, damage);
                Destroy(gameObject);
                break;
            case BulletType.Explosive:
                Explode(other);
                Destroy(gameObject);
                break;
            case BulletType.Ray:
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider col in colliders)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (col.GetComponent<Movement>())
            {
                damagedObjects.Add(damageable);
            }
            if (damageable != null && !damagedObjects.Contains(damageable))
            {
                float finalDamage = (col == impactCollider) ? damage : damage * 0.5f;
                damageable.TakeDamage(finalDamage);
                damagedObjects.Add(damageable);
            }

            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 explosionDirection = (rb.transform.position - transform.position).normalized;

                if (explosionDirection == Vector3.zero)
                {
                    explosionDirection = Vector3.up;
                }

                Movement moveScript = rb.GetComponent<Movement>();
                if (moveScript != null)
                {
                    moveScript.ApplyKnockback(explosionDirection * force);
                }
                else
                {
                    rb.AddExplosionForce(force, transform.position, radius, 1f, ForceMode.Impulse);
                }
            }
        }
    }

}