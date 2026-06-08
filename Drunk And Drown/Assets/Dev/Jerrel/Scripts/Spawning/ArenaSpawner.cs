using System.Collections;
using UnityEngine;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isSpawning = true;
        }
    }

    private void Update()
    {
        if (_isSpawning)
        {
            arenaDuration -= Time.deltaTime;

            if (arenaDuration <= 0f)
            {
                arenaDuration = 0f;
                _isSpawning = false;

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

            SpawnEnemy();
        }
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
    }
}