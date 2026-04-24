using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float hp;

    private void Update()
    {
        DeathCheck();
    }
    private void DeathCheck()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
    public void TakeDamage(float Damage)
    {
        hp -= Damage;
    }
}
