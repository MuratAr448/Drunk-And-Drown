using TMPro;
using UnityEngine;
using System.Collections;

public static class UIAnimationUtils
{
    public static void StartFlyingText(
        MonoBehaviour caller,
        TextMeshProUGUI targetLabel,
        int amount,
        Vector3 worldPosition,
        Color color,
        System.Action onComplete
    )
    {
        if (caller == null || targetLabel == null)
        {
            onComplete?.Invoke();
            return;
        }

        caller.StartCoroutine(FlyTextRoutine(targetLabel, amount, worldPosition, color, onComplete));
    }

    public static Coroutine StartTextBounce(
        MonoBehaviour caller,
        TextMeshProUGUI targetLabel,
        Coroutine activeBounceRoutine,
        float intensityMultiplier = 1f
    )
    {
        if (caller == null || targetLabel == null) return null;

        if (activeBounceRoutine != null)
        {
            caller.StopCoroutine(activeBounceRoutine);
        }

        return caller.StartCoroutine(BounceTextRoutine(targetLabel, intensityMultiplier));
    }

    private static IEnumerator FlyTextRoutine(
        TextMeshProUGUI targetLabel,
        int amount,
        Vector3 worldPosition,
        Color color,
        System.Action onComplete
    )
    {
        Canvas canvas = targetLabel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // Create the floating text GameObject
        GameObject flyingGo = new GameObject("FlyingText");
        flyingGo.transform.SetParent(canvas.transform, false);

        // Disable raycasts on this floating text
        CanvasGroup canvasGroup = flyingGo.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        TextMeshProUGUI flyingText = flyingGo.AddComponent<TextMeshProUGUI>();
        flyingText.font = targetLabel.font;
        flyingText.fontSharedMaterial = targetLabel.fontSharedMaterial;
        float intensityMultiplier = Mathf.Clamp(1f + (amount - 10) * 0.02f, 1f, 2.5f);
        flyingText.fontSize = 36f * intensityMultiplier;
        flyingText.color = color;
        flyingText.alignment = TextAlignmentOptions.Center;
        flyingText.text = "+" + amount;

        RectTransform flyingRect = flyingGo.GetComponent<RectTransform>();
        flyingRect.sizeDelta = new Vector2(200f * intensityMultiplier, 50f * intensityMultiplier);

        // Convert world position to screen position
        Vector2 startScreenPos;
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPosition);
            if (screenPos.z < 0)
            {
                Object.Destroy(flyingGo);
                onComplete?.Invoke();
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
        RectTransform targetRect = targetLabel.rectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetRect.position),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out targetLocalPoint
        );

        float duration = 0.53f;
        float elapsed = 0f;
        Vector2 startPos = localStartPoint;

        // Curved arc path for a beautiful float/fly trajectory
        Vector2 controlPoint = (startPos + targetLocalPoint) / 2f + new Vector2(Random.Range(-80f, 80f), Random.Range(100f, 150f)) * intensityMultiplier;

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

        Object.Destroy(flyingGo);
        onComplete?.Invoke();
    }

    private static IEnumerator BounceTextRoutine(TextMeshProUGUI targetLabel, float intensityMultiplier)
    {
        RectTransform labelRect = targetLabel.rectTransform;
        labelRect.localScale = Vector3.one;

        float duration = 0.08f;
        float elapsed = 0f;
        float maxScale = 1f + 0.3f * intensityMultiplier;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            labelRect.localScale = Vector3.one * Mathf.Lerp(1f, maxScale, t);
            yield return null;
        }

        elapsed = 0f;
        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            labelRect.localScale = Vector3.one * Mathf.Lerp(maxScale, 1f, t);
            yield return null;
        }

        labelRect.localScale = Vector3.one;
    }
}
