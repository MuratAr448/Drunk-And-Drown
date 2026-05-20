using UnityEngine;

public enum EnemyState { Idle, Patrol, Chase, Attack }

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float Health;
    public float MaxHealth;

    [Header("Score")]
    [SerializeField] protected int _scoreOnDeath = 10;

    [Header("State Management")]
    [SerializeField] protected EnemyState _currentState;

    [Header("Patrol Settings")]
    [SerializeField] protected float _patrolRange = 10f;
    [SerializeField] protected float _moveSpeed = 3f;
    [SerializeField] protected float _waypointTolerance = 0.5f;
    [SerializeField] protected float _minIdleTime = 1f;
    [SerializeField] protected float _maxIdleTime = 2f;

    [Header("Detection Settings")]
    [SerializeField] protected float _detectionRange = 8f;
    [SerializeField] protected float _attackRange = 2f;

    public float CurrentHealth => Health;
    public float BaseHealth => MaxHealth;

    private Rigidbody rb;
    private Vector3 _spawnPoint;
    private Vector3 _targetWaypoint;
    private bool _hasWaypoint;
    private float _idleTimer;
    private float _currentIdleDuration;
    protected Transform _playerTransform;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    private void Start()
    {
        _spawnPoint = transform.position;
        _currentState = EnemyState.Patrol;
    }

    private void Update()
    {
        CheckForPlayer();

        switch (_currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                AttackState();
                break;
        }
    }

    private void CheckForPlayer()
    {
        if (_playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer <= _attackRange)
        {
            _currentState = EnemyState.Attack;
        }
        else if (distanceToPlayer <= _detectionRange)
        {
            _currentState = EnemyState.Chase;
        }
        else if (_currentState == EnemyState.Chase || _currentState == EnemyState.Attack)
        {
            _currentState = EnemyState.Patrol;
            _hasWaypoint = false;
        }
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    public void ResetHealth()
    {
        Health = MaxHealth;
    }

    public void Die()
    {
        ComboSystem.Instance.OnEnemyKilled();
        ScoreSystem.Instance.AddScore(_scoreOnDeath);
        Destroy(gameObject);
    }

    public abstract void Attack();

    protected void Idle()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        _idleTimer += Time.deltaTime;

        if (_idleTimer >= _currentIdleDuration)
        {
            _idleTimer = 0f;
            _currentState = EnemyState.Patrol;
        }
    }

    protected void Patrol()
    {
        if (!_hasWaypoint)
        {
            Vector2 randomCircle = Random.insideUnitCircle * _patrolRange;
            _targetWaypoint = new Vector3(
                _spawnPoint.x + randomCircle.x,
                _spawnPoint.y,
                _spawnPoint.z + randomCircle.y
            );
            _hasWaypoint = true;
        }

        MoveTowards(_targetWaypoint);

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(_targetWaypoint.x, 0, _targetWaypoint.z)
        );

        if (distance < _waypointTolerance)
        {
            _hasWaypoint = false;
            _idleTimer = 0f;
            _currentIdleDuration = Random.Range(_minIdleTime, _maxIdleTime);
            _currentState = EnemyState.Idle;
        }
    }

    protected void Chase()
    {
        if (_playerTransform != null)
        {
            MoveTowards(_playerTransform.position);
        }
    }

    protected void AttackState()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        if (_playerTransform != null)
        {
            Vector3 lookDir = (_playerTransform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(lookDir.x, 0, lookDir.z));
            rb.MoveRotation(targetRotation);
        }
        Attack();
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 moveDir = (target - transform.position).normalized;
        Vector3 velocity = moveDir * _moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDir.x, 0, moveDir.z));
            rb.MoveRotation(targetRotation);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? _spawnPoint : transform.position;
        Gizmos.DrawWireSphere(center, _patrolRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}