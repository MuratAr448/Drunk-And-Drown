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
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
        }

        Die();
    }

}
