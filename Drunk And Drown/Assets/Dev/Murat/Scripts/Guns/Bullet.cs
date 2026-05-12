using System.Collections;
using UnityEngine;
public enum BulletType
{
    Normal,
    Exsplosive
}
public class Bullet : MonoBehaviour
{
    public BulletType type;
    public float damage = 1;
    public float timeTillDeath = 1;
    public float radius = 1;
    public float force = 1;
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
        switch (type)
        {
            case BulletType.Normal:
                if (other.CompareTag("Enemy") && other.GetComponent<Enemy>())
                {
                    Enemy enemy = other.GetComponent<Enemy>();
                    enemy.TakeDamage(damage);
                    Destroy(gameObject);
                }
                break;
            case BulletType.Exsplosive:
                // explotion
                if (!other.CompareTag("Player"))
                {
                    Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
                    foreach (Collider collider in colliders)
                    {
                        Rigidbody rb = collider.GetComponent<Rigidbody>();

                        if (rb != null)
                        {
                            if (rb.GetComponent<Movement>() != null)
                            {
                                //rb.GetComponent<Movement>().Impact(transform.position,force* 10);
                            }
                            else
                            {
                                rb.AddExplosionForce(force*10, transform.position, radius,force*10,ForceMode.Impulse);
                            }
                        }
                    }
                    /*
                    Enemy enemy = other.GetComponent<Enemy>();
                    enemy.TakeDamage(damage);*/
                    Destroy(gameObject);
                }

                break;
        }
    }
}
