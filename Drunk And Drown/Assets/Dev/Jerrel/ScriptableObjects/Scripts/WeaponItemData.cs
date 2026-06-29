using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Shop/weapon Item")]
public class WeaponItemData : ShopItemData
{
    public GameObject weaponPrefab;

    public override bool TryPurchase(MainPlayer player)
    {
        if (weaponPrefab == null)
        {
            return false;
        }

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

            // Check if player already has this weapon type
            Weapons prefabComp = weaponPrefab.GetComponent<Weapons>();
            System.Type targetType = prefabComp != null ? prefabComp.GetType() : null;
            int existingIndex = -1;
            bool isParrotGun = weaponPrefab.GetComponent<ParrotGun>() != null;

            if (isParrotGun && MainPlayer.Instance != null)
            {
                ParrotGun existingParrot = MainPlayer.Instance.GetComponentInChildren<ParrotGun>(true);
                if (existingParrot != null)
                {
                    // Find if it has a null entry in weapons list (since ParrotGun has no Weapons component)
                    if (MainPlayer.weapons != null)
                    {
                        for (int i = 0; i < MainPlayer.weapons.Count; i++)
                        {
                            if (MainPlayer.weapons[i] == null)
                            {
                                existingIndex = i;
                                break;
                            }
                        }
                    }

                    // If not tracked in the weapons list yet, destroy it directly so the new one can take its place
                    if (existingIndex == -1)
                    {
                        Destroy(existingParrot.gameObject);
                    }
                }
            }

            if (existingIndex == -1 && MainPlayer.weapons != null)
            {
                string cleanPrefabName = weaponPrefab.name.Replace("(Clone)", "").Trim();
                for (int i = 0; i < MainPlayer.weapons.Count; i++)
                {
                    if (MainPlayer.weapons[i] != null)
                    {
                        if (targetType != null && MainPlayer.weapons[i].GetType() == targetType)
                        {
                            existingIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        // Check name matching in Hotbar for non-Weapons components
                        if (MainPlayer.HotbarInstance != null && i < MainPlayer.HotbarInstance.childCount)
                        {
                            Transform child = MainPlayer.HotbarInstance.GetChild(i);
                            if (child != null && child.name.Replace("(Clone)", "").Trim() == cleanPrefabName)
                            {
                                existingIndex = i;
                                break;
                            }
                        }
                    }
                }
            }

            if (existingIndex != -1)
            {
                MainPlayer.ReplaceWeapon(existingIndex, spawnedWeapon);
            }
            else
            {
                MainPlayer.AddWeaponToList(spawnedWeapon);
            }

            return true;
        }
        return false;
    }
}
