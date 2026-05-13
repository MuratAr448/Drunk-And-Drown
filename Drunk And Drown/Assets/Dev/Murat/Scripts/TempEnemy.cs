using UnityEngine;

public class TempEnemy : MonoBehaviour
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
