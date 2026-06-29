using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public static class RaritySystem
{
    public static float GetMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Uncommon: return 1.2f;
            case ItemRarity.Rare: return 1.5f;
            case ItemRarity.Epic: return 2.0f;
            case ItemRarity.Legendary: return 3.0f;
            default: return 1.0f;
        }
    }

    public static string GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Uncommon: return "#55FF55";   // Green
            case ItemRarity.Rare: return "#5555FF";       // Blue
            case ItemRarity.Epic: return "#AA00FF";       // Purple
            case ItemRarity.Legendary: return "#FF9900";  // Orange
            default: return "#BBBBBB";                    // Grey (Common)
        }
    }

    public static ItemRarity RollRarity(float luck)
    {
        // Base weights: Common=40%, Uncommon=25%, Rare=18%, Epic=12%, Legendary=5%
        // Higher luck decreases common weight and increases rarer weights
        float wCommon = 40f / Mathf.Max(0.1f, luck);
        float wUncommon = 25f;
        float wRare = 18f * Mathf.Sqrt(luck);
        float wEpic = 12f * luck;
        float wLegendary = 5f * luck * luck;

        float totalWeight = wCommon + wUncommon + wRare + wEpic + wLegendary;
        float roll = Random.Range(0f, totalWeight);

        if (roll < wCommon) return ItemRarity.Common;
        roll -= wCommon;
        if (roll < wUncommon) return ItemRarity.Uncommon;
        roll -= wUncommon;
        if (roll < wRare) return ItemRarity.Rare;
        roll -= wRare;
        if (roll < wEpic) return ItemRarity.Epic;
        return ItemRarity.Legendary;
    }

    public static void ApplyRarity(ShopItemData item, ItemRarity rarity)
    {
        item.rarity = rarity;
        float mult = GetMultiplier(rarity);

        // Color code and prefix the item name with its rarity
        item.itemName = $"<color={GetRarityColor(rarity)}>[{rarity}]</color> {item.itemName}";

        if (item is UpgradeItemData upgrade)
        {
            // Upgrades: Scale all positive stats
            upgrade.healthIncrease = Mathf.Round(upgrade.healthIncrease * mult);
            upgrade.speedIncrease = upgrade.speedIncrease * mult;
            upgrade.healAmount = Mathf.Round(upgrade.healAmount * mult);
            upgrade.luckIncrease = upgrade.luckIncrease * mult;
            upgrade.damageMultiplierIncrease = upgrade.damageMultiplierIncrease * mult;
        }
        else if (item is ModifierItemData modifier)
        {
            // Modifiers: Only scale the BUFFS, do not affect DEBUFFS
            // Buffs: coinReward, coinMultiplierIncrease, speedIncrease (if > 0f), scoreMultiplierIncrease
            modifier.coinReward = Mathf.RoundToInt(modifier.coinReward * mult);
            modifier.coinMultiplierIncrease = modifier.coinMultiplierIncrease * mult;
            modifier.scoreMultiplierIncrease = modifier.scoreMultiplierIncrease * mult;
            
            if (modifier.speedIncrease > 0f)
            {
                modifier.speedIncrease = modifier.speedIncrease * mult;
            }

            // Note: healthDecrease, maxEnemyCountIncrease, enemyHealthMultiplierIncrease, and speedIncrease (if < 0f) remain at their base values.
        }
    }
}

public abstract class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int cost;

    [Header("Rarity Settings")]
    public ItemRarity rarity = ItemRarity.Common;

    public abstract bool TryPurchase(MainPlayer player);
}