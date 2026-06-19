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

        return true;
    }
}
