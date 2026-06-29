using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject _shopUI;
    
    public bool IsShopActive => _shopUI != null && _shopUI.activeSelf;

    [Header("Reroll Settings")]
    [SerializeField] private Button _rerollButton;
    [SerializeField] private TextMeshProUGUI _rerollText;
    private int _currentRerollCost = 10;

    [Header("Shop Layout")]
    [SerializeField] private GameObject shopSlotPrefab;
    [SerializeField] private Transform weaponsContainer;
    [SerializeField] private Transform upgradesContainer;
    [SerializeField] private Transform modifiersContainer;

    [Header("Available Items to Generate")]
    [SerializeField] private List<WeaponItemData> availableWeapons;
    public List<WeaponItemData> AvailableWeapons => availableWeapons;
    [SerializeField] private List<UpgradeItemData> availableUpgrades;
    [SerializeField] private List<ModifierItemData> availableModifiers;

    public void ToggleShop()
    {
        bool shouldBeActive = !_shopUI.activeSelf;

        _shopUI.SetActive(shouldBeActive);

        Movement playerMovement = null;
        if (MainPlayer.Instance != null)
        {
            playerMovement = MainPlayer.Instance.GetComponent<Movement>();
        }

        if (shouldBeActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            if (playerMovement != null)
            {
                playerMovement.canMove = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            if (playerMovement != null)
            {
                playerMovement.canMove = true;
            }
        }
    }

    private void Update()
    {
        if (_shopUI != null && _shopUI.activeSelf)
        {
            UpdateRerollUI();

            if (Input.GetKeyDown(KeyCode.R))
            {
                RerollShop();
            }
        }
    }

    private void UpdateRerollUI()
    {
        bool canAfford = CoinSystem.Instance != null && CoinSystem.Instance.GetCoinAmount() >= _currentRerollCost;

        if (_rerollText != null)
        {
            string costColor = canAfford ? "#55FF55" : "#FF5555";
            string textColor = canAfford ? "#FFFFFF" : "#888888";
            _rerollText.text = $"<color={textColor}>Reroll (<color={costColor}>{UIUtils.FormatNumber(_currentRerollCost)} Coins</color>)</color>";
        }
        
        if (_rerollButton != null)
        {
            _rerollButton.interactable = canAfford;
        }
    }

    public void RerollShop()
    {
        if (CoinSystem.Instance == null) return;

        int playerCoins = CoinSystem.Instance.GetCoinAmount();
        if (playerCoins >= _currentRerollCost)
        {
            CoinSystem.Instance.AddCoins(-_currentRerollCost);
            
            // Generate shop with new items but keep current reroll state
            GenerateShop(true);

            // Increase cost by 10 for the next reroll
            _currentRerollCost += 10;
            UpdateRerollUI();
        }
    }

    public void OpenShopWithRandomItems()
    {
        GenerateShop(false);

        // Open the shop UI if not already active
        if (!_shopUI.activeSelf)
        {
            ToggleShop();
        }
    }

    public void GenerateShop(bool isReroll = false)
    {
        if (!isReroll)
        {
            _currentRerollCost = 10;
        }

        // Allow all weapons to appear (even if already owned) so they can be replaced
        List<WeaponItemData> filteredWeapons = new List<WeaponItemData>();
        if (availableWeapons != null)
        {
            foreach (var weaponData in availableWeapons)
            {
                if (weaponData != null && weaponData.weaponPrefab != null)
                {
                    filteredWeapons.Add(weaponData);
                }
            }
        }

        // If player owns all weapons, show a "SOLD OUT" slot
        if (filteredWeapons.Count == 0)
        {
            WeaponItemData soldOutItem = ScriptableObject.CreateInstance<WeaponItemData>();
            soldOutItem.itemName = "SOLD OUT";
            soldOutItem.cost = 0;
            soldOutItem.icon = null;
            soldOutItem.rarity = ItemRarity.Common;
            filteredWeapons.Add(soldOutItem);
        }

        // Assign items to existing slots in the containers
        PopulateSlots(weaponsContainer, filteredWeapons, 1);
        PopulateSlots(upgradesContainer, availableUpgrades, 2);
        PopulateSlots(modifiersContainer, availableModifiers, 3);

        UpdateRerollUI();
    }

    private void PopulateSlots<T>(Transform container, List<T> availableItems, int count) where T : ShopItemData
    {
        if (container == null)
        {
            Debug.LogError("ShopManager: Container is null!");
            return;
        }

        if (availableItems == null || availableItems.Count == 0)
        {
            Debug.LogWarning($"ShopManager: No available items assigned for container {container.name}!");
            return;
        }

        // Find all ShopSlot children in this container
        ShopSlot[] slots = container.GetComponentsInChildren<ShopSlot>(true);
        if (slots.Length == 0)
        {
            Debug.LogWarning($"ShopManager: No ShopSlot components found inside container '{container.name}'!");
            return;
        }

        // Get random items
        List<T> selectedItems = GetRandomElements(availableItems, count);

        // Assign and initialize each slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < selectedItems.Count)
            {
                // Instantiate a runtime copy so we don't overwrite the original ScriptableObject asset
                T itemCopy = Instantiate(selectedItems[i]);

                // Roll and apply rarity based on current player Luck
                float luck = MainPlayer.Instance != null ? MainPlayer.Instance.Luck : 1f;
                ItemRarity rolledRarity = RaritySystem.RollRarity(luck);
                RaritySystem.ApplyRarity(itemCopy, rolledRarity);

                // Reset scale in case it was shrunk to zero in a previous purchase
                slots[i].transform.localScale = Vector3.one;
                slots[i].gameObject.SetActive(true);
                slots[i].Initialize(itemCopy);
            }
            else
            {
                // Deactivate any extra slots if we don't have enough selected items
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    private List<T> GetRandomElements<T>(List<T> sourceList, int count) where T : ScriptableObject
    {
        List<T> result = new List<T>();
        if (sourceList == null || sourceList.Count == 0) return result;

        List<T> temp = new List<T>(sourceList);
        int itemsToSelect = Mathf.Min(count, temp.Count);

        for (int i = 0; i < itemsToSelect; i++)
        {
            int index = Random.Range(0, temp.Count);
            result.Add(temp[index]);
            temp.RemoveAt(index);
        }

        // Fill remaining slots if count > sourceList.Count
        while (result.Count < count && sourceList.Count > 0)
        {
            result.Add(sourceList[Random.Range(0, sourceList.Count)]);
        }

        return result;
    }
}