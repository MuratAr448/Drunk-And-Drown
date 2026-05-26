using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ShopItem : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int _itemCost;

    public void OnPointerDown(PointerEventData eventData)
    {
        TryBuy();
    }

    protected void TryBuy()
    {
        if (!CanAfford())
        {
            Debug.Log("Can't afford");
            return;
        }

        CoinSystem.Instance.AddCoins(-_itemCost);
        GiveItem();
    }

    protected bool CanAfford()
    {
        return CoinSystem.Instance.GetCoinAmount() >= _itemCost;
    }

    public abstract void GiveItem();
}