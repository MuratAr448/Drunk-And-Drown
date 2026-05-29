using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int cost;

    public enum ItemType { Weapon, Upgrade, Modifier }
    public ItemType itemType;
}