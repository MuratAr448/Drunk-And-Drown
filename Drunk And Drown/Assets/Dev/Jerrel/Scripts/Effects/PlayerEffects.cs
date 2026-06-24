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

    [Header("View Bobbing Settings")]
    [SerializeField] private float bobSpeed = 12f;
    [SerializeField] private float bobAmount = 0.04f;
    [SerializeField] private float bobHorizontalAmount = 0.02f;
    private float bobTimer = 0f;
    private Vector3 bobbingOffset = Vector3.zero;

    [Header("Camera Shake Settings")]
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;
    private float currentShakeTime = 0f;
    private Vector3 shakeOffset = Vector3.zero;
    private Vector3 shakeRotationOffset = Vector3.zero;

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

    public void ShakeCamera(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        currentShakeTime = duration;
    }

    private void HandleViewBobbing()
    {
        if (movementScript == null) return;

        bool isGrounded = movementScript.isGrounded;
        Vector3 velocity = movementScript.GetComponent<Rigidbody>().linearVelocity;
        float horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;

        if (isGrounded && horizontalSpeed > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed * (horizontalSpeed / movementScript.WalkSpeed);
            float bobX = Mathf.Cos(bobTimer) * bobHorizontalAmount;
            float bobY = Mathf.Sin(bobTimer * 2f) * bobAmount;
            bobbingOffset = new Vector3(bobX, bobY, 0f);
        }
        else
        {
            bobTimer = 0f;
            bobbingOffset = Vector3.Lerp(bobbingOffset, Vector3.zero, Time.deltaTime * 10f);
        }
    }

    private void HandleCameraShake()
    {
        if (currentShakeTime > 0f)
        {
            currentShakeTime -= Time.deltaTime;
            float percentComplete = currentShakeTime / shakeDuration;
            float currentIntensity = shakeMagnitude * percentComplete;

            float shakeX = Random.Range(-1f, 1f) * currentIntensity;
            float shakeY = Random.Range(-1f, 1f) * currentIntensity;
            float shakeZ = Random.Range(-1f, 1f) * currentIntensity;
            shakeOffset = new Vector3(shakeX, shakeY, shakeZ);

            float rotX = Random.Range(-1f, 1f) * currentIntensity * 10f;
            float rotY = Random.Range(-1f, 1f) * currentIntensity * 10f;
            float rotZ = Random.Range(-1f, 1f) * currentIntensity * 10f;
            shakeRotationOffset = new Vector3(rotX, rotY, rotZ);
        }
        else
        {
            shakeOffset = Vector3.zero;
            shakeRotationOffset = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (playerCamera == null) return;

        HandleViewBobbing();
        HandleCameraShake();

        playerCamera.transform.localPosition += bobbingOffset + shakeOffset;

        if (shakeRotationOffset != Vector3.zero)
        {
            playerCamera.transform.localRotation *= Quaternion.Euler(shakeRotationOffset);
        }
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