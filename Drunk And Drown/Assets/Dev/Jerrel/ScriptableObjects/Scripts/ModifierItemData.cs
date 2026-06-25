using UnityEngine;

[CreateAssetMenu(fileName = "NewModifierItem", menuName = "Shop/Modifier Item")]
public class ModifierItemData : ShopItemData
{
    public float healthDecrease;
    public float speedIncrease;
    public int coinReward;
    public float coinMultiplierIncrease;
    public int maxEnemyCountIncrease;
    public float luckIncrease;

    [Header("Enemy & Score Multipliers")]
    [Tooltip("Increases enemy health by a multiplier (e.g. 0.2 means +20% health)")]
    public float enemyHealthMultiplierIncrease;
    [Tooltip("Increases global score multiplier (e.g. 0.5 means +50% score)")]
    public float scoreMultiplierIncrease;

    public override bool TryPurchase(MainPlayer player)
    {
        CoinSystem.Instance.AddCoins(coinReward);
        CoinSystem.Instance.AddCoinMultiplier(coinMultiplierIncrease);
        player.ModifyMaxHealth(-healthDecrease);
        player.Luck += luckIncrease;

        if (player.TryGetComponent<Movement>(out var movement))
        {
            movement.ModifySpeed(speedIncrease);
        }

        if (maxEnemyCountIncrease != 0)
        {
            var spawners = FindObjectsByType<ArenaSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var spawner in spawners)
            {
                spawner.MaxSpawns += maxEnemyCountIncrease;
            }
        }

        if (enemyHealthMultiplierIncrease > 0f)
        {
            Enemy.GlobalHealthMultiplier += enemyHealthMultiplierIncrease;
            
            // Scale currently active enemies
            Enemy[] existingEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var enemy in existingEnemies)
            {
                enemy.MaxHealth *= (1f + enemyHealthMultiplierIncrease);
                enemy.Health *= (1f + enemyHealthMultiplierIncrease);
                enemy.OnHealthChanged?.Invoke();
            }
        }

        if (scoreMultiplierIncrease > 0f && ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.AddScoreMultiplier(scoreMultiplierIncrease);
        }

        return true;
    }
}
