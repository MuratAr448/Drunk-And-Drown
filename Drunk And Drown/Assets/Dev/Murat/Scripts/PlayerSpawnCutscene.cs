using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawnCutscene : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private float duration = 4.0f;
    [SerializeField] private bool playOnStart = true;

    [Header("Wake Up Animation Curves")]
    [SerializeField] private AnimationCurve standingCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),         // Starts at 0, moving up
        new Keyframe(0.7f, 1.15f, 0f, 0f),    // Over shoots to 1.15 (swing forward)
        new Keyframe(0.85f, 0.95f, 0f, 0f),   // Under shoots to 0.95 (swing back)
        new Keyframe(1f, 1f, 0f, 0f)          // Settles at 1.0
    );
    [SerializeField] private AnimationCurve fadeCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.2f, 1f),
        new Keyframe(1f, 0f)
    );

    [Header("Wake Up Options")]
    [SerializeField] private float startTilt = 35f; // Side tilt (head roll)
    [SerializeField] private float startHeightOffset = -0.5f; // Starts closer to the ground
    
    private Movement movement;
    private Camera playerCam;
    
    private CanvasGroup fadeCanvasGroup;
    private Image fadeImage;

    void Start()
    {
        movement = GetComponent<Movement>();
        playerCam = GetComponentInChildren<Camera>();

        if (playOnStart)
        {
            StartCutscene();
        }
    }

    public void StartCutscene()
    {
        if (movement == null || playerCam == null)
        {
            Debug.LogWarning("PlayerSpawnCutscene: Movement or Camera reference is missing!");
            return;
        }
        StartCoroutine(CutsceneCoroutine());
    }

    private IEnumerator CutsceneCoroutine()
    {
        // 1. Disable the player movement script to prevent input/height updates
        movement.enabled = false;

        // 2. Set up the dynamic Canvas overlay for the eye blinks
        CreateFadeOverlay();

        // Save normal camera default local position
        Vector3 defaultLocalPos = playerCam.transform.localPosition;
        Vector3 startingLocalPos = defaultLocalPos + new Vector3(0, startHeightOffset, 0);

        // Record initial camera tilt
        playerCam.transform.localPosition = startingLocalPos;
        playerCam.transform.localRotation = Quaternion.Euler(0, 0, startTilt);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Evaluate animation curves
            float tStanding = standingCurve.Evaluate(t);
            float alpha = fadeCurve.Evaluate(t);

            // Interpolate camera tilt and position to default standing state
            playerCam.transform.localPosition = Vector3.Lerp(startingLocalPos, defaultLocalPos, tStanding);
            playerCam.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(0, 0, startTilt), Quaternion.identity, tStanding);

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = alpha;
            }

            yield return null;
        }

        // Clean up
        playerCam.transform.localPosition = defaultLocalPos;
        playerCam.transform.localRotation = Quaternion.identity;

        if (fadeCanvasGroup != null)
        {
            Destroy(fadeCanvasGroup.gameObject);
        }

        // Re-enable player movement
        movement.enabled = true;
    }

    private void CreateFadeOverlay()
    {
        GameObject canvasGo = new GameObject("CutsceneFadeCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGo.AddComponent<CanvasScaler>();

        GameObject imageGo = new GameObject("FadeImage");
        imageGo.transform.SetParent(canvasGo.transform, false);
        
        fadeImage = imageGo.AddComponent<Image>();
        fadeImage.color = Color.black;

        // Stretch image to fill screen
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        fadeCanvasGroup = canvasGo.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 1f;
    }
}
