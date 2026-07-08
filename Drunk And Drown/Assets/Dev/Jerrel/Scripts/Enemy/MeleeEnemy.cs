using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MeleeEnemy : Enemy
{
    private bool _hasAttacked;
    [SerializeField]
    private float _attackSpeed = 1f;
    [SerializeField]
    private float _attackRadius = 2f;
    [SerializeField]
    private int _attackDamage = 10;
    [SerializeField]
    private Vector3 _attackOffset = Vector3.forward;
    [Tooltip("Chance (from 0 to 1) that the attack sound plays on swing.")]
    [Range(0f, 1f)] [SerializeField] private float _attackSoundChance = 0.5f;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Attack()
    {
        if (_hasAttacked) return;

        // Play random attack sound with configured probability
        if (enemySounds != null && Random.value <= _attackSoundChance && audioSource != null)
        {
            Sounds(1);
        }

        Vector3 attackPoint = transform.position + transform.TransformDirection(_attackOffset);

        Collider[] hitColliders = Physics.OverlapSphere(attackPoint, _attackRadius);

        foreach (Collider collider in hitColliders)
        {
            IDamageable damageable = collider.GetComponent<IDamageable>();

            if (damageable != null && collider.gameObject != gameObject)
            { 
                damageable.TakeDamage(_attackDamage);
            }
        }

        StartCoroutine(AttackCooldown());
    }

    public override void Die()
    {
        // Play death sound on a temporary GameObject so it doesn't get cut off when the enemy is destroyed
        if (enemySounds != null)
        {
            GameObject tempAudioGo = new GameObject("TempAudio_EnemyDeath_" + gameObject.name);
            tempAudioGo.transform.position = transform.position;
            AudioSource tempSource = tempAudioGo.AddComponent<AudioSource>();
            
            // Set 3D spatial properties
            tempSource.spatialBlend = 1.0f;
            tempSource.minDistance = 1f;
            tempSource.maxDistance = 30f;

            enemySounds[2].Play(tempSource);
            
            float duration = tempSource.clip != null ? tempSource.clip.length : 1f;
            Destroy(tempAudioGo, duration);
        }

        base.Die();
    }

    private IEnumerator AttackCooldown()
    {
        _hasAttacked = true;
        yield return new WaitForSeconds(_attackSpeed);
        _hasAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 attackPoint = transform.position + transform.TransformDirection(_attackOffset);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint, _attackRadius);
    }
}