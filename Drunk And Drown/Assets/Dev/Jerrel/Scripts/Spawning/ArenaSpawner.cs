using System.Collections;
using UnityEngine;
using TMPro;

public class ArenaSpawner : MonoBehaviour
{
    [Tooltip("If player walks into the collider, _isSpawning will be true and the script will start to spawn enemies")]
    [SerializeField] private bool _isSpawning = false;
    private bool _canSpawn = true;

    [Tooltip("Max Enemies that are allowed to spawn")]
    [SerializeField] private int _maxSpawns = 10;
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private Transform[] _spawnPoints;

    [Tooltip("Amount of seconds to wait before spawn can trigger")]
    [SerializeField] private float _spawnRate = 0.5f;
    [Tooltip("The amount of time for the arena to be finished")]
    [SerializeField] private float arenaDuration = 60f;

    [Header("UI References")]
    [Tooltip("Text element to display the remaining arena duration")]
    [SerializeField] private TextMeshProUGUI _arenaTimerText;
    [Tooltip("Text element to display the number of remaining enemies")]
    [SerializeField] private TextMeshProUGUI _enemiesLeftText;

    [Header("UI Animation Settings")]
    [Tooltip("The RectTransform of the HUD card/panel containing the text")]
    [SerializeField] private RectTransform _arenaCardRect;
    [SerializeField] private float _slideInDuration = 0.4f;
    [SerializeField] private float _offScreenOffset = 500f;

    private Vector2 _originalCardPos;

    private void Start()
    {
        if (_arenaCardRect != null)
        {
            _originalCardPos = _arenaCardRect.anchoredPosition;
            // Position off-screen initially
            _arenaCardRect.anchoredPosition = _originalCardPos + new Vector2(_offScreenOffset, 0f);
            _arenaCardRect.gameObject.SetActive(_isSpawning);
            
            if (_isSpawning)
            {
                StartCoroutine(SlideInCoroutine());
            }
        }
        else
        {
            if (_arenaTimerText != null)
            {
                _arenaTimerText.gameObject.SetActive(_isSpawning);
            }
            if (_enemiesLeftText != null)
            {
                _enemiesLeftText.gameObject.SetActive(_isSpawning);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !_isSpawning)
        {
            _isSpawning = true;
            if (_arenaCardRect != null)
            {
                StartCoroutine(SlideInCoroutine());
            }
            else
            {
                if (_arenaTimerText != null)
                {
                    _arenaTimerText.gameObject.SetActive(true);
                }
                if (_enemiesLeftText != null)
                {
                    _enemiesLeftText.gameObject.SetActive(true);
                }
            }
        }
    }

    private void Update()
    {
        if (_isSpawning)
        {
            // Only decrease duration if it hasn't reached 0 yet
            if (arenaDuration > 0f)
            {
                arenaDuration -= Time.deltaTime;
                if (arenaDuration <= 0f)
                {
                    arenaDuration = 0f;
                }
            }

            int enemyCount = EnemyUtils.Instance != null ? EnemyUtils.Instance.GetEnemyCount() : 0;

            if (arenaDuration > 0f)
            {
                if (_arenaTimerText != null)
                {
                    _arenaTimerText.text = $"Arena ends in: {Mathf.CeilToInt(arenaDuration)}s";
                }
                if (_enemiesLeftText != null)
                {
                    _enemiesLeftText.text = $"Current enemies: {enemyCount}";
                }
            }
            else
            {
                if (_arenaTimerText != null)
                {
                    _arenaTimerText.text = "Clear all remaining enemies!";
                }
                if (_enemiesLeftText != null)
                {
                    _enemiesLeftText.text = $"Enemies left: {enemyCount}";
                }
            }

            // Only spawn enemies if the timer is still running
            if (arenaDuration > 0f)
            {
                SpawnEnemy();
            }

            // Finish the arena only when the timer has run out AND all enemies are dead
            if (arenaDuration <= 0f && enemyCount <= 0)
            {
                _isSpawning = false;

                if (_arenaCardRect != null)
                {
                    StartCoroutine(SlideOutCoroutine());
                }
                else
                {
                    if (_arenaTimerText != null)
                    {
                        _arenaTimerText.gameObject.SetActive(false);
                    }
                    if (_enemiesLeftText != null)
                    {
                        _enemiesLeftText.gameObject.SetActive(false);
                    }
                }

                // Disable trigger/collider so it doesn't run again
                if (TryGetComponent<Collider>(out var col))
                {
                    col.enabled = false;
                }

                // Find the ShopManager and open the shop with random items
                ShopManager shopManager = FindFirstObjectByType<ShopManager>();
                if (shopManager != null)
                {
                    shopManager.OpenShopWithRandomItems();
                }

                enabled = false; // Disable this script component
                return;
            }
        }
    }

    private IEnumerator SlideInCoroutine()
    {
        if (_arenaCardRect == null) yield break;

        Vector2 targetPosition = _originalCardPos;
        Vector2 startPosition = targetPosition + new Vector2(_offScreenOffset, 0f);

        _arenaCardRect.anchoredPosition = startPosition;
        _arenaCardRect.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < _slideInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(elapsed / _slideInDuration);
            // Smooth ease out curve
            float t = percent * (2f - percent); 
            _arenaCardRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        _arenaCardRect.anchoredPosition = targetPosition;
    }

    private IEnumerator SlideOutCoroutine()
    {
        if (_arenaCardRect == null) yield break;

        Vector2 startPosition = _originalCardPos;
        Vector2 targetPosition = startPosition + new Vector2(_offScreenOffset, 0f);

        float elapsed = 0f;
        while (elapsed < _slideInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(elapsed / _slideInDuration);
            // Smooth ease in curve
            float t = percent * percent; 
            _arenaCardRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        _arenaCardRect.anchoredPosition = targetPosition;
        _arenaCardRect.gameObject.SetActive(false);
    }

    private IEnumerator SpawnCooldown()
    {
        _canSpawn = false;
        yield return new WaitForSeconds(_spawnRate);
        _canSpawn = true;
    }

    private void SpawnEnemy()
    {
        if (_canSpawn && EnemyUtils.Instance.GetEnemyCount() < _maxSpawns)
        {
            GameObject spawnedEnemy = Instantiate(GetRandomEnemy(), GetRandomTransform().position, Quaternion.identity);

            if (spawnedEnemy.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.SpawnedByArena();
            }

            StartCoroutine(SpawnCooldown());
            EnemyUtils.Instance.AddEnemy();
        }
    }

    private GameObject GetRandomEnemy()
    {
        return _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
    }

    private Transform GetRandomTransform()
    {
        return _spawnPoints[Random.Range(0, _spawnPoints.Length)];
    }

    public void SetSpawning(bool isSpawning)
    {
        _isSpawning = isSpawning;
        if (_arenaCardRect != null)
        {
            if (isSpawning)
            {
                StartCoroutine(SlideInCoroutine());
            }
            else
            {
                StartCoroutine(SlideOutCoroutine());
            }
        }
        else
        {
            if (_arenaTimerText != null)
            {
                _arenaTimerText.gameObject.SetActive(isSpawning);
            }
            if (_enemiesLeftText != null)
            {
                _enemiesLeftText.gameObject.SetActive(isSpawning);
            }
        }
    }
}