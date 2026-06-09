using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradeItem", menuName = "Shop/Upgrade Item")]
public class UpgradeItemData : ShopItemData
{
    public float healthIncrease;
    public float speedMultiplier = 1f;

    public override bool TryPurchase(MainPlayer player)
    {
        if (CoinSystem.Instance.GetCoinAmount() >= cost)
        {
            CoinSystem.Instance.AddCoins(-cost);
            player.ModifyMaxHealth(healthIncrease);
            if (player.TryGetComponent<Movement>(out var movement))
            {
                movement.ModifySpeed(speedMultiplier);
            }
            return true;
        }
        return false;
    }
}
