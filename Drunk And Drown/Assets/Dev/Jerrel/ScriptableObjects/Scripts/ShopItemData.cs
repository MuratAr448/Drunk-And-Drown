using UnityEngine;

public abstract class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int cost;

    public abstract bool TryPurchase(MainPlayer player);
}