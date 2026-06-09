using TMPro;
using UnityEngine;
using System.Collections;

public class CoinSystem : MonoBehaviour
{
    [SerializeField] private int _coins;
    [SerializeField] private float _coinMultiplier = 1.0f;

    [SerializeField] private TextMeshProUGUI _coinTextLabel;

    public static CoinSystem Instance;

    private Coroutine _bounceRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        // Keep UI updated in case of other changes
        UpdateUI();
    }

    public void AddCoins(int amount, Vector3? worldPosition = null)
    {
        int finalAmount = amount;
        if (amount > 0)
        {
            finalAmount = Mathf.RoundToInt(amount * _coinMultiplier);
        }

        if (finalAmount == 0) return;

        // Play flying animation if gaining coins and we have a world position
        if (finalAmount > 0 && _coinTextLabel != null && worldPosition.HasValue)
        {
            StartCoroutine(FlyCoinRoutine(finalAmount, worldPosition.Value));
        }
        else
        {
            _coins += finalAmount;
            UpdateUI();
            if (finalAmount != 0 && _coinTextLabel != null)
            {
                TriggerTextBounce();
            }
        }
    }

    /// <summary>
    /// Base coin multiplier is 1x. To increase add to amount, to decrease make amount negative.
    /// </summary>
    /// <param name="amount"></param>
    public void AddCoinMultiplier(float amount)
    {
        _coinMultiplier += amount;
    }

    public int GetCoinAmount()
    {
        return _coins;
    }

    private void UpdateUI()
    {
        if (_coinTextLabel != null)
        {
            _coinTextLabel.text = _coins.ToString();
        }
    }

    private IEnumerator FlyCoinRoutine(int amount, Vector3 worldPosition)
    {
        if (_coinTextLabel == null) yield break;

        Canvas canvas = _coinTextLabel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            _coins += amount;
            UpdateUI();
            TriggerTextBounce();
            yield break;
        }

        // Create the floating text GameObject
        GameObject flyingGo = new GameObject("FlyingCoin");
        flyingGo.transform.SetParent(canvas.transform, false);

        // Disable raycasts on this floating text
        CanvasGroup canvasGroup = flyingGo.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        TextMeshProUGUI flyingText = flyingGo.AddComponent<TextMeshProUGUI>();
        flyingText.font = _coinTextLabel.font;
        flyingText.fontSharedMaterial = _coinTextLabel.fontSharedMaterial;
        flyingText.fontSize = 32f;
        flyingText.color = new Color(1f, 0.82f, 0f); // Gold color
        flyingText.alignment = TextAlignmentOptions.Center;
        flyingText.text = "+" + amount;

        RectTransform flyingRect = flyingGo.GetComponent<RectTransform>();
        flyingRect.sizeDelta = new Vector2(200f, 50f);

        // Convert world position to screen position
        Vector2 startScreenPos;
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPosition);
            if (screenPos.z < 0)
            {
                Destroy(flyingGo);
                _coins += amount;
                UpdateUI();
                TriggerTextBounce();
                yield break;
            }
            startScreenPos = screenPos;
        }
        else
        {
            startScreenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }

        // Convert screen point to local point in the canvas
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            startScreenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localStartPoint
        );

        flyingRect.anchoredPosition = localStartPoint;

        // Convert target position to screen, then to local canvas coordinates
        Vector2 targetLocalPoint;
        RectTransform targetRect = _coinTextLabel.rectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetRect.position),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out targetLocalPoint
        );

        float duration = 0.8f;
        float elapsed = 0f;
        Vector2 startPos = localStartPoint;

        // Curved arc path for a beautiful float/fly trajectory
        Vector2 controlPoint = (startPos + targetLocalPoint) / 2f + new Vector2(Random.Range(-80f, 80f), Random.Range(100f, 150f));

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float easeT = t * t * (3f - 2f * t);

            // Bezier curve interpolation
            Vector2 m1 = Vector2.Lerp(startPos, controlPoint, easeT);
            Vector2 m2 = Vector2.Lerp(controlPoint, targetLocalPoint, easeT);
            flyingRect.anchoredPosition = Vector2.Lerp(m1, m2, easeT);

            if (t > 0.6f)
            {
                float fadeT = (t - 0.6f) / 0.4f;
                flyingText.color = new Color(flyingText.color.r, flyingText.color.g, flyingText.color.b, 1f - fadeT);
            }

            flyingRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, easeT);

            yield return null;
        }

        Destroy(flyingGo);

        _coins += amount;
        UpdateUI();
        TriggerTextBounce();
    }

    private void TriggerTextBounce()
    {
        if (_bounceRoutine != null)
        {
            StopCoroutine(_bounceRoutine);
        }
        _bounceRoutine = StartCoroutine(BounceTextRoutine());
    }

    private IEnumerator BounceTextRoutine()
    {
        RectTransform labelRect = _coinTextLabel.rectTransform;
        labelRect.localScale = Vector3.one;

        float duration = 0.12f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            labelRect.localScale = Vector3.one * Mathf.Lerp(1f, 1.3f, t);
            yield return null;
        }

        elapsed = 0f;
        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            labelRect.localScale = Vector3.one * Mathf.Lerp(1.3f, 1f, t);
            yield return null;
        }

        labelRect.localScale = Vector3.one;
        _bounceRoutine = null;
    }
}
