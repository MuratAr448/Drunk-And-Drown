using UnityEngine;

public class EnemyUtils : MonoBehaviour
{
    [SerializeField] private int _enemyCount = 0;
    public static EnemyUtils Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

    }
    public void AddEnemy()
    {
        _enemyCount++;
    }

    public void RemoveEnemy()
    {
        _enemyCount--;
    }

    public int GetEnemyCount()
    {
        return _enemyCount;
    }
}
