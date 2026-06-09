using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    [SerializeField] private int _totalScore = 0;
    [SerializeField] private TextMeshProUGUI _scoreTextLabel;

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
        UpdateScoreUI();
    }

    public void AddScore(int baseScoreAmount, Vector3? worldPosition = null)
    {
        float multiplier = 1f;

        if (ComboSystem.Instance != null)
        {
            multiplier = ComboSystem.Instance.CurrentMultiplier;
        }

        int finalScore = Mathf.RoundToInt(baseScoreAmount * multiplier);

        if (finalScore <= 0) return;

        if (_scoreTextLabel != null && worldPosition.HasValue)
        {
            StartCoroutine(FlyScoreRoutine(finalScore, worldPosition.Value));
        }
        else
        {
            _totalScore += finalScore;
            UpdateScoreUI();
            if (_scoreTextLabel != null)
            {
                TriggerTextBounce();
            }
        }
    }

    public void UpdateScoreUI()
    {
        if (_scoreTextLabel != null)
        {
            _scoreTextLabel.text = _totalScore.ToString();
        }
    }

    private IEnumerator FlyScoreRoutine(int scoreAmount, Vector3 worldPosition)
    {
        if (_scoreTextLabel == null) yield break;

        Canvas canvas = _scoreTextLabel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            _totalScore += scoreAmount;
            UpdateScoreUI();
            TriggerTextBounce();
            yield break;
        }

        // Create the floating text GameObject
        GameObject flyingGo = new GameObject("FlyingScore");
        flyingGo.transform.SetParent(canvas.transform, false);

        TextMeshProUGUI flyingText = flyingGo.AddComponent<TextMeshProUGUI>();
        flyingText.font = _scoreTextLabel.font;
        flyingText.fontSharedMaterial = _scoreTextLabel.fontSharedMaterial;
        flyingText.fontSize = 36f;
        flyingText.color = Color.white;
        flyingText.alignment = TextAlignmentOptions.Center;
        flyingText.text = "+" + scoreAmount;

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
                _totalScore += scoreAmount;
                UpdateScoreUI();
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
        RectTransform targetRect = _scoreTextLabel.rectTransform;
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

        _totalScore += scoreAmount;
        UpdateScoreUI();
        TriggerTextBounce();
    }

    private void TriggerTextBounce()
    {
        if (_bounceRoutine != null)
        {
            StopCoroutine(_bounceRoutine);
        }
        _bounceRoutine = StartCoroutine(BounceScoreTextRoutine());
    }

    private IEnumerator BounceScoreTextRoutine()
    {
        RectTransform labelRect = _scoreTextLabel.rectTransform;
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