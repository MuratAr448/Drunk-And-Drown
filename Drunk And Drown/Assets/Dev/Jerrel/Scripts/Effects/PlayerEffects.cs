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
    [SerializeField] private float maxFOVOffset = 15f;
    [SerializeField] private float minSpeedThreshold = 5f;
    [SerializeField] private float maxSpeedThreshold = 20f;
    [SerializeField] private float fovChangeSpeed = 8f;

    private Movement movementScript;
    private Vignette vignette;

    private Coroutine flashCoroutine;
    private float baseFOV;
    private float targetFOV;

    void Start()
    {
        globalVolume.profile.TryGet(out vignette);
        vignette.intensity.value = 0f;
        movementScript = GetComponent<Movement>();

        if (playerCamera != null)
        {
            baseFOV = playerCamera.fieldOfView;
            targetFOV = baseFOV;
        }
        else
        {
            Debug.LogError("Player Camera is not assigned on PlayerEffects!", this);
        }
    }

    void Update()
    {
        HandleDynamicFOV();
    }

    private void HandleDynamicFOV()
    {
        if (playerCamera == null || movementScript == null) return;

        float currentSpeed = movementScript.GetVelocity();
        float speedFactor = Mathf.InverseLerp(minSpeedThreshold, maxSpeedThreshold, currentSpeed);
        targetFOV = baseFOV + (speedFactor * maxFOVOffset);
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
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