using UnityEngine;

[CreateAssetMenu(fileName = "NewModifierItem", menuName = "Shop/Modifier Item")]
public class ModifierItemData : ShopItemData
{
    public float healthDecrease;
    public float speedMultiplier = 1f;
    public int coinReward;
    public float coinMultiplierIncrease;

    public override bool TryPurchase(MainPlayer player)
    {
        CoinSystem.Instance.AddCoins(coinReward);
        CoinSystem.Instance.AddCoinMultiplier(coinMultiplierIncrease);
        player.ModifyMaxHealth(-healthDecrease);
        if (player.TryGetComponent<Movement>(out var movement))
        {
            movement.ModifySpeed(speedMultiplier);
        }
        return true;
    }
}
