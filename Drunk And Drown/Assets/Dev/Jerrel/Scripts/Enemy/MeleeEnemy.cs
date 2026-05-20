using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public override void Attack()
    {
        if (_hasAttacked) return;

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