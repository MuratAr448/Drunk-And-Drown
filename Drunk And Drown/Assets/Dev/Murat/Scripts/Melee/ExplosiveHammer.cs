using System.Collections.Generic;
using UnityEngine;

public class ExplosiveHammer : MonoBehaviour
{
    public float radius;
    public float force = 1;
    public float damage = 1;
    public List<IDamageable> damagedObjects = new List<IDamageable>();

    public void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider col in colliders)
        {
            Damaged(col);

           Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Movement moveScript = rb.GetComponent<Movement>();
                if (moveScript == null)
                {
                    rb.AddExplosionForce(force, transform.position, radius, 1f, ForceMode.Impulse);
                }
            }
        }
    }
    private void Damaged(Collider impactCollider)
    {
        IDamageable damageable = impactCollider.GetComponent<IDamageable>();
        if (impactCollider.GetComponent<Movement>())
        {
            damagedObjects.Add(damageable);
        }
        if (damageable != null && !damagedObjects.Contains(damageable))
        {
            damageable.TakeDamage(damage);
            damagedObjects.Add(damageable);
        }
    }
}
