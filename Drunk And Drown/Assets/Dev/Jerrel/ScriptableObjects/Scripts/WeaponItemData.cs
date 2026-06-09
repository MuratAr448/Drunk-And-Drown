using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Shop/Weapon Item")]
public class WeaponItemData : ShopItemData
{
    public GameObject weaponPrefab;

    public override bool TryPurchase(MainPlayer player)
    {
        if (CoinSystem.Instance.GetCoinAmount() >= cost)
        {
            CoinSystem.Instance.AddCoins(-cost);
            MainPlayer.AddWeaponToList(Instantiate(weaponPrefab));
            return true;
        }
        return false;
    }
}
