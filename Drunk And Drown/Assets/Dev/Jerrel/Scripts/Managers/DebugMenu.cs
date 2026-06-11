using UnityEngine;
using System.Collections.Generic;

public class DebugMenu : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // Tilde ~ key
    [SerializeField] private KeyCode alternateToggleKey = KeyCode.F1;
    [SerializeField] private List<GameObject> customWeaponPrefabs; // Manual fallback weapon list

    private bool showMenu = false;
    private Rect windowRect = new Rect(50, 50, 500, 480);

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(alternateToggleKey))
        {
            showMenu = !showMenu;

            // Handle cursor locking when menu toggles
            if (showMenu)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Re-lock only if the shop is not open and game is not paused
                bool shopOpen = false;
                ShopManager shopManager = FindFirstObjectByType<ShopManager>();
                if (shopManager != null)
                {
                    // Check if shop UI GameObject is active
                    // Using reflection or simple active check on its UI container
                    var field = shopManager.GetType().GetField("_shopUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        GameObject shopUIObj = (GameObject)field.GetValue(shopManager);
                        if (shopUIObj != null)
                        {
                            shopOpen = shopUIObj.activeSelf;
                        }
                    }
                }
                
                bool paused = MainPlayer.Instance != null ? MainPlayer.Instance.pause : false;

                if (!shopOpen && !paused)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }

    private void OnGUI()
    {
        if (!showMenu) return;

        // Scale styles for a bigger, cleaner menu
        int originalBtnFontSize = GUI.skin.button.fontSize;
        int originalLabelFontSize = GUI.skin.label.fontSize;
        int originalWindowFontSize = GUI.skin.window.fontSize;
        
        GUI.skin.button.fontSize = 18;
        GUI.skin.label.fontSize = 16;
        GUI.skin.window.fontSize = 20;

        windowRect = GUI.Window(999, windowRect, DrawDebugWindow, "Developer Debug Menu");

        // Restore styles so it doesn't affect other GUI code
        GUI.skin.button.fontSize = originalBtnFontSize;
        GUI.skin.label.fontSize = originalLabelFontSize;
        GUI.skin.window.fontSize = originalWindowFontSize;
    }

    private void DrawDebugWindow(int windowID)
    {
        GUILayout.Space(20);

        if (GUILayout.Button("Give All Weapons", GUILayout.Height(55)))
        {
            GiveAllWeapons();
        }

        GUILayout.Space(15);

        if (GUILayout.Button("Give 100 Dabloons", GUILayout.Height(45)))
        {
            if (CoinSystem.Instance != null)
            {
                CoinSystem.Instance.AddCoins(100);
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Restore Full Health", GUILayout.Height(45)))
        {
            if (MainPlayer.Instance != null)
            {
                MainPlayer.Instance.ResetHealth();
            }
        }

        GUILayout.Space(25);
        GUILayout.Label("Controls:");
        GUILayout.Label("- Press [~] or [F1] to close debug menu.");
        
        GUILayout.Space(10);
        if (MainPlayer.Instance != null)
        {
            GUILayout.Label($"Health: {MainPlayer.Instance.CurrentHealth} / {MainPlayer.Instance.BaseHealth}");
            if (CoinSystem.Instance != null)
            {
                GUILayout.Label($"Dabloons: {CoinSystem.Instance.GetCoinAmount()}");
            }
        }

        GUI.DragWindow();
    }

    private void GiveAllWeapons()
    {
        if (MainPlayer.Instance == null)
        {
            Debug.LogWarning("DebugMenu: MainPlayer.Instance is null!");
            return;
        }

        // 1. Gather all weapons from the ShopManager
        ShopManager shopManager = FindFirstObjectByType<ShopManager>();
        List<GameObject> prefabsToInstantiate = new List<GameObject>();

        if (shopManager != null && shopManager.AvailableWeapons != null)
        {
            foreach (var weaponData in shopManager.AvailableWeapons)
            {
                if (weaponData != null && weaponData.weaponPrefab != null)
                {
                    prefabsToInstantiate.Add(weaponData.weaponPrefab);
                }
            }
        }

        // 2. Gather weapons from custom override list
        if (customWeaponPrefabs != null)
        {
            foreach (var customPrefab in customWeaponPrefabs)
            {
                if (customPrefab != null && !prefabsToInstantiate.Contains(customPrefab))
                {
                    prefabsToInstantiate.Add(customPrefab);
                }
            }
        }

        // 3. Instantiate and give the weapons to the player, avoiding duplicates
        int weaponsAddedCount = 0;
        foreach (var prefab in prefabsToInstantiate)
        {
            if (prefab == null) continue;

            // Check if player already has this weapon in their hotbar list
            bool alreadyHas = false;
            foreach (Weapons existingWeapon in MainPlayer.weapons)
            {
                if (existingWeapon != null)
                {
                    // Match by clean name (removing "(Clone)")
                    string existingClean = existingWeapon.gameObject.name.Replace("(Clone)", "").Trim();
                    string prefabClean = prefab.name.Trim();

                    if (existingClean == prefabClean)
                    {
                        alreadyHas = true;
                        break;
                    }
                }
            }

            if (!alreadyHas)
            {
                GameObject newWeaponInstance = Instantiate(prefab);
                MainPlayer.AddWeaponToList(newWeaponInstance);
                weaponsAddedCount++;
            }
        }

        Debug.Log($"DebugMenu: Gave player {weaponsAddedCount} new weapon(s).");
    }
}
