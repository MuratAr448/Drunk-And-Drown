using UnityEngine;

public abstract class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int cost;
    [TextArea(3, 10)] public string description;

    public abstract bool TryPurchase(MainPlayer player);
}