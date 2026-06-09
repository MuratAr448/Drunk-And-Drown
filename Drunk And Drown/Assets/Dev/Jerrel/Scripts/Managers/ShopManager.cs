using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject _shopUI;

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

    public void OpenShopWithRandomItems()
    {
        GenerateShop();

        // Open the shop UI if not already active
        if (!_shopUI.activeSelf)
        {
            ToggleShop();
        }
    }

    public void GenerateShop()
    {
        // Assign items to existing slots in the containers
        PopulateSlots(weaponsContainer, availableWeapons, 1);
        PopulateSlots(upgradesContainer, availableUpgrades, 2);
        PopulateSlots(modifiersContainer, availableModifiers, 3);
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
                // Reset scale in case it was shrunk to zero in a previous purchase
                slots[i].transform.localScale = Vector3.one;
                slots[i].gameObject.SetActive(true);
                slots[i].Initialize(selectedItems[i]);
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