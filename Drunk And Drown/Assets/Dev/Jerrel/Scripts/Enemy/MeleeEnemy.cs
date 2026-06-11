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

    [Header("Sounds")]
    [SerializeField] private AudioEvent _attackSound;
    [SerializeField] private AudioEvent _deathSound;
    [Tooltip("Chance (from 0 to 1) that the attack sound plays on swing.")]
    [Range(0f, 1f)] [SerializeField] private float _attackSoundChance = 0.5f;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            // Configure standard 3D spatial settings for the dynamic source
            _audioSource.spatialBlend = 1.0f;
            _audioSource.minDistance = 1f;
            _audioSource.maxDistance = 20f;
        }
    }

    public override void Attack()
    {
        if (_hasAttacked) return;

        // Play random attack sound with configured probability
        if (_attackSound != null && Random.value <= _attackSoundChance && _audioSource != null)
        {
            _attackSound.Play(_audioSource);
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
        if (_deathSound != null)
        {
            GameObject tempAudioGo = new GameObject("TempAudio_EnemyDeath_" + gameObject.name);
            tempAudioGo.transform.position = transform.position;
            AudioSource tempSource = tempAudioGo.AddComponent<AudioSource>();
            
            // Set 3D spatial properties
            tempSource.spatialBlend = 1.0f;
            tempSource.minDistance = 1f;
            tempSource.maxDistance = 30f;
            
            _deathSound.Play(tempSource);
            
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