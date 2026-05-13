using System.Collections;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] protected Transform _firepoint;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private float _bulletLifetime = 3f;
    [SerializeField] private float _bulletSpeed = 20f;
    private bool _canShoot = true;
    [SerializeField] private float _attackSpeed = 1f;

    protected override void Awake()
    {
        base.Awake();
    }
    public override void Attack()
    {
        if (_playerTransform == null || _firepoint == null || !_canShoot) return;

        _canShoot = false;

        Vector3 targetPosition = _playerTransform.position;
        Vector3 direction = (targetPosition - _firepoint.position).normalized;

        GameObject firedBullet = Instantiate(_bullet, _firepoint.position, Quaternion.identity);

        Rigidbody rb = firedBullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * _bulletSpeed;
        }

        Destroy(firedBullet, _bulletLifetime);
        StartCoroutine(ShootCooldown());
    }

    private IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(_attackSpeed);
        _canShoot = true;
    }
}