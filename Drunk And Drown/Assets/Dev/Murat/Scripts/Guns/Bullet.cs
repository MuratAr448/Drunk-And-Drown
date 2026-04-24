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
    public float damage;
    public float timeTillDeath;
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
                Destroy(gameObject);
                break;
        }
    }
}
