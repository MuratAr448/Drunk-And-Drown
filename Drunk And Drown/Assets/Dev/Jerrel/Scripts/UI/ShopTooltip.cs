using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopTooltip : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    [Header("Positioning Settings")]
    [SerializeField] private Vector2 offset = new Vector2(15f, 15f);

    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (tooltipPanel != null)
        {
            rectTransform = tooltipPanel.GetComponent<RectTransform>();
            
            // Prevent the tooltip panel and its children from intercepting raycasts, which causes flickering
            CanvasGroup canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
            }
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private void Start()
    {
        if (tooltipPanel != null && canvas != null)
        {
            // Reparent to the canvas root so it draws on top of all other elements
            tooltipPanel.transform.SetParent(canvas.transform, true);
            tooltipPanel.transform.SetAsLastSibling();
        }
        Hide();
    }

    private void OnDestroy()
    {
        if (tooltipPanel != null)
        {
            Destroy(tooltipPanel);
        }
    }
    
    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            FollowMouse();
        }
    }

    public void Show(ShopItemData itemData)
    {
        if (itemData == null) return;

        if (tooltipPanel != null)
        {
            tooltipPanel.transform.SetAsLastSibling();
            tooltipPanel.SetActive(true);
        }

        if (nameText != null) nameText.text = itemData.itemName;
        if (descriptionText != null)
        {
            descriptionText.text = GetStatChangesText(itemData);
        }
        if (costText != null)
        {
            if (itemData.itemName == "SOLD OUT")
            {
                costText.text = "N/A";
            }
            else
            {
                costText.text = itemData is ModifierItemData ? "FREE" : $"{UIUtils.FormatNumber(itemData.cost)} Dabloons";
            }
        }

        FollowMouse();
    }

    private string GetStatChangesText(ShopItemData itemData)
    {
        if (itemData != null && itemData.itemName == "SOLD OUT")
        {
            return "You have obtained all available weapons! Set sail with your arsenal!";
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        MainPlayer player = MainPlayer.Instance;
        Movement movement = player != null ? player.GetComponent<Movement>() : null;
        CoinSystem coinSystem = CoinSystem.Instance;

        float currentMaxHP = player != null ? player.BaseHealth : 100f;
        float currentSpeed = movement != null ? movement.WalkSpeed : 6f;
        float currentCoinMultiplier = coinSystem != null ? coinSystem.CoinMultiplier : 1f;

        if (itemData is ModifierItemData modifier)
        {
            sb.AppendLine("<color=#FFA500><b>Stat Modifications:</b></color>");

            // Max Health
            if (modifier.healthDecrease != 0f)
            {
                float oldHP = currentMaxHP;
                float newHP = currentMaxHP - modifier.healthDecrease;
                sb.AppendLine($"- Max Health: {UIUtils.FormatNumber(oldHP)} -> <color=#FF5555>{UIUtils.FormatNumber(newHP)}</color> (-{UIUtils.FormatNumber(modifier.healthDecrease)})");
            }

            // Speed
            if (modifier.speedIncrease != 0f)
            {
                float oldSpeed = currentSpeed;
                float newSpeed = currentSpeed + modifier.speedIncrease;
                string color = modifier.speedIncrease > 0f ? "#55FF55" : "#FF5555";
                string sign = modifier.speedIncrease > 0f ? "+" : "";
                sb.AppendLine($"- Speed: {UIUtils.FormatNumber(oldSpeed)} -> <color={color}>{UIUtils.FormatNumber(newSpeed)}</color> ({sign}{UIUtils.FormatNumber(modifier.speedIncrease)})");
            }

            // Coin Multiplier
            if (modifier.coinMultiplierIncrease != 0f)
            {
                float oldMult = currentCoinMultiplier;
                float newMult = currentCoinMultiplier + modifier.coinMultiplierIncrease;
                string color = modifier.coinMultiplierIncrease > 0f ? "#55FF55" : "#FF5555";
                string sign = modifier.coinMultiplierIncrease > 0f ? "+" : "";
                sb.AppendLine($"- Coin Multiplier: {UIUtils.FormatNumber(oldMult)}x -> <color={color}>{UIUtils.FormatNumber(newMult)}x</color> ({sign}{UIUtils.FormatNumber(modifier.coinMultiplierIncrease)})");
            }

            // Coin Reward
            if (modifier.coinReward != 0)
            {
                string color = modifier.coinReward > 0 ? "#55FF55" : "#FF5555";
                string sign = modifier.coinReward > 0 ? "+" : "";
                sb.AppendLine($"- Coin Reward: <color={color}>{sign}{UIUtils.FormatNumber(modifier.coinReward)}</color> Dabloons");
            }

            // Max Enemy Spawns
            if (modifier.maxEnemyCountIncrease != 0)
            {
                ArenaSpawner sampleSpawner = FindFirstObjectByType<ArenaSpawner>(FindObjectsInactive.Include);
                string color = modifier.maxEnemyCountIncrease > 0 ? "#FF5555" : "#55FF55"; // red for harder (more enemies), green for easier
                string sign = modifier.maxEnemyCountIncrease > 0 ? "+" : "";

                if (sampleSpawner != null)
                {
                    int oldSpawns = sampleSpawner.MaxSpawns;
                    int newSpawns = oldSpawns + modifier.maxEnemyCountIncrease;
                    sb.AppendLine($"- Max Arena Spawns: {UIUtils.FormatNumber(oldSpawns)} -> <color={color}>{UIUtils.FormatNumber(newSpawns)}</color> ({sign}{UIUtils.FormatNumber(modifier.maxEnemyCountIncrease)})");
                }
                else
                {
                    sb.AppendLine($"- Max Arena Spawns: <color={color}>{sign}{UIUtils.FormatNumber(modifier.maxEnemyCountIncrease)}</color> Enemies");
                }
            }

            // Luck
            if (modifier.luckIncrease != 0f)
            {
                float oldLuck = player != null ? player.Luck : 1f;
                float newLuck = oldLuck + modifier.luckIncrease;
                string color = modifier.luckIncrease > 0f ? "#55FF55" : "#FF5555";
                string sign = modifier.luckIncrease > 0f ? "+" : "";
                sb.AppendLine($"- Luck: {UIUtils.FormatNumber(oldLuck)} -> <color={color}>{UIUtils.FormatNumber(newLuck)}</color> ({sign}{UIUtils.FormatNumber(modifier.luckIncrease)})");
            }
        }
        else if (itemData is UpgradeItemData upgrade)
        {
            sb.AppendLine("<color=#00FFFF><b>Upgrade Stats:</b></color>");

            // Max Health
            if (upgrade.healthIncrease != 0f)
            {
                float oldHP = currentMaxHP;
                float newHP = currentMaxHP + upgrade.healthIncrease;
                string color = upgrade.healthIncrease > 0f ? "#55FF55" : "#FF5555";
                string sign = upgrade.healthIncrease > 0f ? "+" : "";
                sb.AppendLine($"- Max Health: {UIUtils.FormatNumber(oldHP)} -> <color={color}>{UIUtils.FormatNumber(newHP)}</color> ({sign}{UIUtils.FormatNumber(upgrade.healthIncrease)})");
            }

            // Speed
            if (upgrade.speedIncrease != 0f)
            {
                float oldSpeed = currentSpeed;
                float newSpeed = currentSpeed + upgrade.speedIncrease;
                string color = upgrade.speedIncrease > 0f ? "#55FF55" : "#FF5555";
                string sign = upgrade.speedIncrease > 0f ? "+" : "";
                sb.AppendLine($"- Speed: {UIUtils.FormatNumber(oldSpeed)} -> <color={color}>{UIUtils.FormatNumber(newSpeed)}</color> ({sign}{UIUtils.FormatNumber(upgrade.speedIncrease)})");
            }

            // Healing
            if (upgrade.healAmount > 0f)
            {
                float oldHP = player != null ? player.CurrentHealth : 100f;
                float newHP = Mathf.Min(oldHP + upgrade.healAmount, currentMaxHP + upgrade.healthIncrease);
                sb.AppendLine($"- Healing: {UIUtils.FormatNumber(oldHP)} -> <color=#55FF55>{UIUtils.FormatNumber(newHP)}</color> (+{UIUtils.FormatNumber(upgrade.healAmount)})");
            }

            // Luck
            if (upgrade.luckIncrease != 0f)
            {
                float oldLuck = player != null ? player.Luck : 1f;
                float newLuck = oldLuck + upgrade.luckIncrease;
                string color = upgrade.luckIncrease > 0f ? "#55FF55" : "#FF5555";
                string sign = upgrade.luckIncrease > 0f ? "+" : "";
                sb.AppendLine($"- Luck: {UIUtils.FormatNumber(oldLuck)} -> <color={color}>{UIUtils.FormatNumber(newLuck)}</color> ({sign}{UIUtils.FormatNumber(upgrade.luckIncrease)})");
            }
        }
        else if (itemData is WeaponItemData weaponItem && weaponItem.weaponPrefab != null)
        {
            Weapons baseWeapon = weaponItem.weaponPrefab.GetComponent<Weapons>();
            if (baseWeapon != null)
            {
                sb.AppendLine("<color=#FF00FF><b>Weapon Stats:</b></color>");

                float mult = RaritySystem.GetMultiplier(weaponItem.rarity);

                float baseDamage = baseWeapon.GetDamage();
                float scaledDamage = baseDamage * mult;
                if (baseDamage > 0f)
                {
                    sb.AppendLine($"- Damage: {UIUtils.FormatNumber(baseDamage)} -> <color=#55FF55>{UIUtils.FormatNumber(scaledDamage)}</color>");
                }

                float baseRate1 = baseWeapon.GetRate1();
                float scaledRate1 = baseRate1 / mult;
                if (baseRate1 > 0f)
                {
                    sb.AppendLine($"- {baseWeapon.GetRate1Name()}: {UIUtils.FormatNumber(baseRate1)}s -> <color=#55FF55>{UIUtils.FormatNumber(scaledRate1)}s</color>");
                }

                float baseRate2 = baseWeapon.GetRate2();
                float scaledRate2 = baseRate2 / mult;
                if (baseRate2 > 0f)
                {
                    sb.AppendLine($"- {baseWeapon.GetRate2Name()}: {UIUtils.FormatNumber(baseRate2)}s -> <color=#55FF55>{UIUtils.FormatNumber(scaledRate2)}s</color>");
                }
            }
        }

        return sb.ToString();
    }

    public void Hide()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void FollowMouse()
    {
        if (rectTransform == null || canvas == null) return;

        // Force canvas layout to rebuild immediately to get accurate rect dimensions for dynamic content
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) parentRect = canvas.transform as RectTransform;

        Vector2 mousePos = Input.mousePosition;

        // Convert mouse position to local point in parentRect
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            mousePos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localMousePos
        );

        // Bounding box dimensions of the tooltip
        float w = rectTransform.rect.width;
        float h = rectTransform.rect.height;
        Vector2 pivot = rectTransform.pivot;

        // Decide horizontal positioning based on screen space availability
        float targetX;
        // Check if tooltip fits to the right of the cursor
        if (localMousePos.x + offset.x + (1f - pivot.x) * w <= parentRect.rect.xMax)
        {
            targetX = localMousePos.x + offset.x;
        }
        else
        {
            // Position to the left of the cursor
            targetX = localMousePos.x - offset.x - (1f - pivot.x) * w;
        }

        // Decide vertical positioning based on screen space availability
        float targetY;
        // Check if tooltip fits above the cursor
        if (localMousePos.y + offset.y + (1f - pivot.y) * h <= parentRect.rect.yMax)
        {
            targetY = localMousePos.y + offset.y;
        }
        else
        {
            // Position below the cursor
            targetY = localMousePos.y - offset.y - (1f - pivot.y) * h;
        }

        // Clamp target position so the tooltip bounds stay inside parentRect bounds
        float minX = parentRect.rect.xMin + pivot.x * w;
        float maxX = parentRect.rect.xMax - (1f - pivot.x) * w;
        float minY = parentRect.rect.yMin + pivot.y * h;
        float maxY = parentRect.rect.yMax - (1f - pivot.y) * h;

        targetX = Mathf.Clamp(targetX, minX, maxX);
        targetY = Mathf.Clamp(targetY, minY, maxY);

        rectTransform.anchoredPosition = new Vector2(targetX, targetY);
    }
}
