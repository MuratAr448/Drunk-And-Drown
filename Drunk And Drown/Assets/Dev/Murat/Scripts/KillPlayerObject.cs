using UnityEngine;

public class KillPlayerObject : MonoBehaviour
{
    private float timeForDamage = 0;
    private void Update()
    {
        timeForDamage += Time.deltaTime;
        if (timeForDamage > 1)
        {
            lava();
            timeForDamage = 0;
        }
    }
    public void lava()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position,new Vector3(transform.localScale.x*0.5f, transform.localScale.y, transform.localScale.z * 0.5f));

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out IDamageable Damaged))
            {
                Damaged.TakeDamage(5);
            }
        }
    }
}
