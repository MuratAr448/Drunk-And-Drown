using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<TempEnemy> enemyList = new List<TempEnemy>();
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
                if (other.GetComponent<TempEnemy>() )
                {
                    TempEnemy enemy = other.GetComponent<TempEnemy>();
                    if (!enemyList.Contains(enemy))
                    {
                        enemy.TakeDamage(damage);
                        enemyList.Add(enemy);
                        Destroy(gameObject);
                    }
                }
                break;
            case BulletType.Exsplosive:
                // explotion
                if (other.GetComponent<TempEnemy>())
                {
                    TempEnemy enemy = other.GetComponent<TempEnemy>();
                    if (!enemyList.Contains(enemy))
                    {
                        enemy.TakeDamage(damage);
                        enemyList.Add(enemy);
                        enemy.GetComponent<Rigidbody>().AddForce((transform.forward+Vector3.up*0.5f) * force * 2.5f, ForceMode.Impulse);
                    }
                }
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
                                rb.GetComponent<Movement>().Exposion();
                            }
                            rb.AddExplosionForce(force, transform.position, radius,force*0.1f,ForceMode.Impulse);
                            if (rb.GetComponent<TempEnemy>() != null)
                            {
                                TempEnemy enemy = rb.GetComponent<TempEnemy>();
                                if (!enemyList.Contains(enemy))
                                {
                                    enemy.TakeDamage(damage*0.5f);
                                    enemyList.Add(enemy);
                                }
                            }
                        }
                    }
                    Destroy(gameObject);
                }
                break;
            default: break;
        }
    }
}
