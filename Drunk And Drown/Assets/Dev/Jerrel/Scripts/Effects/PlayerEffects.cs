using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerEffects : MonoBehaviour
{
    [Header("Volume Setup")]
    [SerializeField] private Volume globalVolume;

    [Header("Vignette Settings")]
    [SerializeField] private float intensityOnHit = 0.5f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Camera Effects")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float targetFOVOffset = 15f;
    [SerializeField] private float fovFadeDuration = 0.25f;

    private Movement movementScript;
    private Vignette vignette;

    private Coroutine flashCoroutine;
    private Coroutine fovCoroutine;
    private float baseFOV;

    void Start()
    {
        baseFOV = playerCamera.fieldOfView;
        globalVolume.profile.TryGet(out vignette);
        vignette.intensity.value = 0f;
        movementScript = GetComponent<Movement>();
    }

    public void TriggerFOVFade()
    {
        if (fovCoroutine != null)
        {
            StopCoroutine(fovCoroutine);
        }

        fovCoroutine = StartCoroutine(FadeFOV());
    }

    private IEnumerator FadeFOV()
    {
        float startFOV = baseFOV;
        float targetFOV = baseFOV + targetFOVOffset;
        float elapsedTime = 0f;

        playerCamera.fieldOfView = targetFOV;

        while (elapsedTime < fovFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            playerCamera.fieldOfView = Mathf.Lerp(targetFOV, startFOV, elapsedTime / fovFadeDuration);
            yield return null;
        }
        playerCamera.fieldOfView = startFOV;
    }

    public void TakeDamageFlash(Color vignetteColor)
    {
        vignette.color.value = vignetteColor;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FadeVignette());
    }

    private IEnumerator FadeVignette()
    {
        vignette.intensity.value = intensityOnHit;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(intensityOnHit, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        vignette.intensity.value = 0f;
    }
}