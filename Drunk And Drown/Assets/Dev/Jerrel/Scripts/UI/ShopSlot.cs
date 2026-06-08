using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;

    [SerializeField] private ShopItemData currentItemData;

    private void Start()
    {
        SetupSlot();
    }
    public void SetupSlot()
    {
        iconImage.sprite = currentItemData.icon;
        nameText.text = currentItemData.itemName;
        costText.text = $"{currentItemData.cost} Dabloons";
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            TryPurchase();
        }
    }

    private void TryPurchase()
    {
        if (CanAfford())
        {
            switch (currentItemData.itemType)
            {
                case ShopItemData.ItemType.Weapon:
                    break;
                case ShopItemData.ItemType.Upgrade:
                    break;
                case ShopItemData.ItemType.Modifier:
                    break;
                default:
                    break;
            }
        }
    }

    private bool CanAfford()
    {
        return CoinSystem.Instance.GetCoinAmount() > currentItemData.cost;
    }
}