using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Shop/weapon Item")]
public class WeaponItemData : ShopItemData
{
    public GameObject weaponPrefab;

    public override bool TryPurchase(MainPlayer player)
    {
        if (CoinSystem.Instance.GetCoinAmount() >= cost)
        {
            CoinSystem.Instance.AddCoins(-cost);
            GameObject spawnedWeapon = Instantiate(weaponPrefab);
            Weapons weaponComponent = spawnedWeapon.GetComponent<Weapons>();
            if (weaponComponent != null)
            {
                float multiplier = RaritySystem.GetMultiplier(rarity);
                weaponComponent.ApplyRarityScaling(multiplier);
            }
            MainPlayer.AddWeaponToList(spawnedWeapon);
            return true;
        }
        return false;
    }
}
