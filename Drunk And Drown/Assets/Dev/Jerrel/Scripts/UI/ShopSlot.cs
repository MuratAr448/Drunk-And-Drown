using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ShopSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;

    [SerializeField] private ShopItemData currentItemData;

    private ShopTooltip localTooltip;

    private MainPlayer playerRef;
    private bool isPurchased = false;
    private bool isShaking = false;

    private void Start()
    {
        if (localTooltip == null)
        {
            localTooltip = GetComponent<ShopTooltip>();
        }
        if (playerRef == null)
        {
            playerRef = FindFirstObjectByType<MainPlayer>();
        }
        if (currentItemData != null)
        {
            SetupSlot();
        }
    }

    public void Initialize(ShopItemData itemData)
    {
        currentItemData = itemData;
        isPurchased = false;
        if (playerRef == null)
        {
            playerRef = FindFirstObjectByType<MainPlayer>();
        }
        SetupSlot();
    }

    public void SetupSlot()
    {
        if (currentItemData == null)
        {
            Debug.LogWarning("ShopSlot: SetupSlot called but currentItemData is null.");
            return;
        }
        if (iconImage != null) iconImage.sprite = currentItemData.icon;

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (backgroundImage != null)
        {
            Color rarityColor;
            if (ColorUtility.TryParseHtmlString(RaritySystem.GetRarityColor(currentItemData.rarity), out rarityColor))
            {
                rarityColor.a = backgroundImage.color.a;
                backgroundImage.color = rarityColor;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isPurchased || currentItemData == null) return;
        if (localTooltip != null)
        {
            localTooltip.Show(currentItemData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (localTooltip != null)
        {
            localTooltip.Hide();
        }
    }

    private void OnDisable()
    {
        if (localTooltip != null)
        {
            localTooltip.Hide();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPurchased) return;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            TryPurchase();
        }
    }

    private void TryPurchase()
    {
        if (playerRef != null && !isPurchased)
        {
            if (currentItemData.TryPurchase(playerRef))
            {
                StartCoroutine(PurchaseSuccessAnimation());
            }
            else
            {
                StartCoroutine(ShakeAnimation());
            }
        }
    }

    private IEnumerator ShakeAnimation()
    {
        if (isShaking) yield break;
        isShaking = true;

        Vector3 originalPos = transform.localPosition;
        float duration = 0.25f;
        float elapsed = 0f;
        float magnitude = 8f;

        while (elapsed < duration)
        {
            float percentComplete = elapsed / duration;
            float damper = 1.0f - percentComplete;

            float x = Random.Range(-1f, 1f) * magnitude * damper;
            float y = Random.Range(-1f, 1f) * magnitude * damper;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        isShaking = false;
    }

    private IEnumerator PurchaseSuccessAnimation()
    {
        isPurchased = true;

        Vector3 originalScale = transform.localScale;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float percentComplete = elapsed / duration;
            float scaleFactor;

            if (percentComplete < 0.3f)
            {
                float subPercent = percentComplete / 0.3f;
                scaleFactor = Mathf.Lerp(1.0f, 1.15f, subPercent);
            }
            else
            {
                float subPercent = (percentComplete - 0.3f) / 0.7f;
                scaleFactor = Mathf.Lerp(1.15f, 0.0f, subPercent);
            }

            transform.localScale = originalScale * scaleFactor;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}