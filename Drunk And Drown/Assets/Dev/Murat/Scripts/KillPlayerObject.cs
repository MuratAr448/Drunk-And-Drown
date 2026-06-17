using UnityEngine;

public class KillPlayerObject : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out IDamageable Damaged))
        {
            Damaged.TakeDamage(15000);
        }
    }
}
