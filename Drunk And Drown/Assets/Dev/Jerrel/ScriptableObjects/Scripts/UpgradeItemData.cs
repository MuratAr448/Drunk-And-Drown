using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradeItem", menuName = "Shop/Upgrade Item")]
public class UpgradeItemData : ShopItemData
{
    public float healthIncrease;
    public float speedMultiplier = 1f;
    public float healAmount;
    public float luckIncrease;

    public override bool TryPurchase(MainPlayer player)
    {
        if (CoinSystem.Instance.GetCoinAmount() >= cost)
        {
            CoinSystem.Instance.AddCoins(-cost);
            player.ModifyMaxHealth(healthIncrease);
            
            if (healAmount > 0f)
            {
                player.Heal(healAmount);
            }

            if (player.TryGetComponent<Movement>(out var movement))
            {
                movement.ModifySpeed(speedMultiplier);
            }

            player.Luck += luckIncrease;
            return true;
        }
        return false;
    }
}
