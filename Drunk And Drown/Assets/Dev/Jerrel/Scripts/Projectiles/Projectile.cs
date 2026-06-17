using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float _damage;

    protected void Die()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore other enemies to disable friendly fire and let bullets pass through
        if (collision.gameObject.GetComponent<Enemy>() != null)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            return;
        }

        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
        }

        Die();
    }

}
