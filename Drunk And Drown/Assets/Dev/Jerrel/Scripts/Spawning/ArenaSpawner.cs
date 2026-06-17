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
    
    public int MaxSpawns
    {
        get => _maxSpawns;
        set => _maxSpawns = value;
    }
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

    private static readonly System.Collections.Generic.Dictionary<RectTransform, Vector2> OriginalCardPositions = 
        new System.Collections.Generic.Dictionary<RectTransform, Vector2>();

    private Vector2 GetOriginalCardPos()
    {
        if (_arenaCardRect != null)
        {
            if (!OriginalCardPositions.ContainsKey(_arenaCardRect))
            {
                OriginalCardPositions[_arenaCardRect] = _arenaCardRect.anchoredPosition;
            }
            return OriginalCardPositions[_arenaCardRect];
        }
        return Vector2.zero;
    }

    private static readonly System.Collections.Generic.List<ArenaSpawner> ActiveSpawners = new System.Collections.Generic.List<ArenaSpawner>();

    private void RegisterActive()
    {
        if (!ActiveSpawners.Contains(this))
        {
            ActiveSpawners.Add(this);
        }

        if (ActiveSpawners.Count == 1)
        {
            if (_arenaCardRect != null)
            {
                _arenaCardRect.gameObject.SetActive(true);
                StartCoroutine(SlideInCoroutine());
            }
            else
            {
                if (_arenaTimerText != null) _arenaTimerText.gameObject.SetActive(true);
                if (_enemiesLeftText != null) _enemiesLeftText.gameObject.SetActive(true);
            }
        }
    }

    private void UnregisterActive()
    {
        ActiveSpawners.Remove(this);

        if (ActiveSpawners.Count == 0)
        {
            if (_arenaCardRect != null)
            {
                StartCoroutine(SlideOutCoroutine());
            }
            else
            {
                if (_arenaTimerText != null) _arenaTimerText.gameObject.SetActive(false);
                if (_enemiesLeftText != null) _enemiesLeftText.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        ActiveSpawners.Remove(this);
    }

    private float _initialArenaDuration;

    private void Awake()
    {
        _initialArenaDuration = arenaDuration;
    }

    private void Start()
    {
        if (_arenaCardRect != null)
        {
            Vector2 origPos = GetOriginalCardPos();
            // Position off-screen initially if nothing is spawning yet
            if (ActiveSpawners.Count == 0)
            {
                _arenaCardRect.anchoredPosition = origPos + new Vector2(_offScreenOffset, 0f);
                _arenaCardRect.gameObject.SetActive(false);
            }
        }

        if (_isSpawning)
        {
            RegisterActive();
        }
    }

    public static void ResetAllArenas()
    {
        ActiveSpawners.Clear();

        ArenaSpawner[] spawners = FindObjectsByType<ArenaSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ArenaSpawner spawner in spawners)
        {
            spawner._isSpawning = false;
            spawner.arenaDuration = spawner._initialArenaDuration;
            spawner._canSpawn = true;
            spawner.enabled = true;
            if (spawner.TryGetComponent<Collider>(out var col))
            {
                col.enabled = true;
            }

            if (spawner._arenaCardRect != null)
            {
                Vector2 origPos = spawner.GetOriginalCardPos();
                spawner._arenaCardRect.anchoredPosition = origPos + new Vector2(spawner._offScreenOffset, 0f);
                spawner._arenaCardRect.gameObject.SetActive(false);
            }
            else
            {
                if (spawner._arenaTimerText != null) spawner._arenaTimerText.gameObject.SetActive(false);
                if (spawner._enemiesLeftText != null) spawner._enemiesLeftText.gameObject.SetActive(false);
            }
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        if (EnemyUtils.Instance != null)
        {
            EnemyUtils.Instance.ResetEnemyCount();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !_isSpawning)
        {
            _isSpawning = true;
            RegisterActive();
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

            // Only the first active spawner updates the UI
            if (ActiveSpawners.Count > 0 && ActiveSpawners[0] == this)
            {
                float maxDuration = 0f;
                bool anyTimerRunning = false;
                foreach (var spawner in ActiveSpawners)
                {
                    if (spawner.arenaDuration > 0f)
                    {
                        anyTimerRunning = true;
                        if (spawner.arenaDuration > maxDuration)
                        {
                            maxDuration = spawner.arenaDuration;
                        }
                    }
                }

                if (anyTimerRunning)
                {
                    if (_arenaTimerText != null)
                    {
                        _arenaTimerText.text = $"Arena ends in: {Mathf.CeilToInt(maxDuration)}s";
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
                UnregisterActive();

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

        Vector2 targetPosition = GetOriginalCardPos();
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

        Vector2 startPosition = GetOriginalCardPos();
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
        if (_isSpawning == isSpawning) return;

        _isSpawning = isSpawning;
        if (isSpawning)
        {
            RegisterActive();
        }
        else
        {
            UnregisterActive();
        }
    }
}