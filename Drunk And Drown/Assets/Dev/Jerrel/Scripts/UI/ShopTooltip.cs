using UnityEngine;
using TMPro;

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
        canvas = GetComponentInParent<Canvas>();
        Hide();
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

        if (nameText != null) nameText.text = itemData.itemName;
        if (descriptionText != null) descriptionText.text = itemData.description;
        if (costText != null)
        {
            costText.text = itemData is ModifierItemData ? "FREE" : $"{itemData.cost} Dabloons";
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }
        FollowMouse();
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

        Vector2 mousePos = Input.mousePosition;
        Vector2 screenPos = mousePos + (offset * canvas.scaleFactor);

        // Calculate tooltip dimensions scaled to screen resolution
        float width = rectTransform.rect.width * canvas.scaleFactor;
        float height = rectTransform.rect.height * canvas.scaleFactor;

        Vector2 pivot = rectTransform.pivot;
        float minX = pivot.x * width;
        float maxX = Screen.width - ((1f - pivot.x) * width);
        float minY = pivot.y * height;
        float maxY = Screen.height - ((1f - pivot.y) * height);

        // Clamp in screen coordinates
        screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
        screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

        // Convert the clamped screen position to parent's local space coordinates
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) parentRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
    }
}
